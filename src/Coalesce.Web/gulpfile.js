/// <binding ProjectOpened='default' />
/*
This file in the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkId=518007
*/

var gulp = require('gulp'),
    sourcemaps = require('gulp-sourcemaps'),
    sassCompiler = require('gulp-sass')(require('sass')),
    typescriptCompiler = require('gulp-typescript'),
    del = require('del'),
    shell = require('gulp-shell'),
    plumber = require('gulp-plumber'), // Handles Gulp errors (https://www.npmjs.com/package/gulp-plumber)
    fs = require('fs');

// Initialize directory paths.
var paths = {
    // Source Directory Paths
    npm: "./node_modules/",
    scripts: "Scripts/",
    styles: "Styles/",
    wwwroot: "./wwwroot/",
    images: "Images/"
};
// Destination Directory Paths
paths.css = paths.wwwroot + "/css/";
paths.fonts = paths.wwwroot + "/fonts/";
paths.img = paths.wwwroot + "/img/";
paths.js = paths.wwwroot + "/js/";
paths.lib = paths.wwwroot + "/lib/";


exports.cleanImages = function cleanLib() {
    return del(paths.img);
};

exports.copyImages = gulp.series(exports.cleanImages, function copyStatic() {
    return gulp
        .src(paths.images + '**/*.{png,jpg,ico}')
        .pipe(gulp.dest(paths.img));
});

exports.watchImages = function watchImages() { 
    return gulp.watch(paths.images + '**/*.{png,jpg,ico}', exports.copyImages);
};

const npm = {
    "bootstrap": "bootstrap-sass/assets/**/bootstrap*.{js,map}",
    "bootstrap/fonts": "bootstrap-sass/assets/fonts/**/*.{,eot,svg,ttf,woff,woff2}",
    "jquery": "jquery/dist/*.{js,map}",
    "font-awesome": "components-font-awesome/**/*.{css,otf,eot,svg,ttf,woff,woff2}",
    "moment": "moment/moment.js",
    "knockout": "knockout/build/output/*.js",
    "knockout-validation": "knockout.validation/dist/*.js",
    "select2": "select2/dist/**/*.{css,js}",
    "select2-bootstrap": "select2-bootstrap-theme/dist/*.css",
    "bootstrap-datetimepicker": "eonasdan-bootstrap-datetimepicker/build/**/*.{css,js}"
};
exports.cleanLib = function cleanLib() {
    return del(paths.lib);
};
exports.copyLib = gulp.series(exports.cleanLib, gulp.parallel(
    Object
        .keys(npm)
        .map(destinationDir => {
            const fn = function () {
                return gulp
                    .src(paths.npm + npm[destinationDir])
                    .pipe(gulp.dest(paths.lib + destinationDir));
            };
            fn.displayName = `copyLib:${destinationDir}`;
            return fn;
        })
    )
);

exports.copyJs = function copyJs() {
    return gulp.src(paths.scripts + "*.js")
        .pipe(gulp.dest(paths.js));
};

exports.watchJs = function watchJs() { return gulp.watch(paths.scripts + '*.js', exports.copyJs) };

exports.sass = function sass() {
    return gulp
        .src(paths.styles + '*.scss')
        .pipe(plumber())
        .pipe(sassCompiler().on('error', sassCompiler.logError))
        .pipe(gulp.dest(paths.css));
};

exports.watchSass = function watchSass() { return gulp.watch([paths.styles + '*.scss'], exports.sass) };

exports.tsLocal = function tsLocal() {
    var individualFileTypescriptProject = typescriptCompiler.createProject('tsconfig.json', {
        typescript: require('typescript')
    });

    //var individualFileTypescriptProject = typescriptCompiler.createProject('tsconfig.json');
    return gulp.src([paths.scripts + '*.ts', '!' + paths.scripts + '{coalesce,Ko,ko}*.ts'])
        .pipe(sourcemaps.init())
        .pipe(individualFileTypescriptProject(typescriptCompiler.reporter.fullReporter(true)))
        .js
        .pipe(sourcemaps.write('.')).pipe(gulp.dest(paths.js));
};

exports.ts = gulp.parallel(exports.tsLocal, function ts() {
    // compile the root generated code into an app.js file
    var rootAppJsProject = typescriptCompiler.createProject('tsconfig.json', { outFile: 'app.js' });
    return gulp.src([paths.scripts + 'viewmodels.generated.d.ts'])
        .pipe(plumber())
        .pipe(sourcemaps.init())
        .pipe(rootAppJsProject())
        .js
        .pipe(plumber())
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest(paths.js));
});

exports.copyTs = gulp.series(exports.ts, function copyTs() {
    return gulp.src(paths.scripts + "*.{ts,js.map}")
        .pipe(plumber())
        .pipe(gulp.dest(paths.js));
});

exports.watchTs = gulp.parallel(
    function watchScripts() { return gulp.watch([paths.scripts + '**/*.ts'], exports.tsLocal) },
    function watchPartials() { return gulp.watch([paths.scripts + 'Partials/*.ts'], exports.ts) }
);

exports.copyFiles = gulp.parallel(
    exports.sass,
    exports.copyLib,
    exports.copyImages,
    exports.copyTs,
    exports.copyJs,
);
exports.copyAll = exports.copyFiles;

exports.watch = gulp.parallel(
    exports.watchSass,
    exports.watchTs,
    exports.watchImages,
    exports.watchJs
);

exports.default = gulp.series(
    exports.copyAll, exports.watch
);


var coalesceBuildDir = `${require('os').tmpdir()}/CoalesceExe`;
var dotnetCoalesce = `dotnet "${coalesceBuildDir}/dotnet-coalesce.dll"`;

exports.coalesceCleanBuild = function coalesceCleanBuild() {
    return del(coalesceBuildDir, { force: true });
};

exports.coalesceBuild = gulp.series(
    exports.coalesceCleanBuild,
    shell.task([
            'dotnet restore --verbosity quiet "../IntelliTect.Coalesce.DotnetTool"',
            `dotnet build "../IntelliTect.Coalesce.DotnetTool/IntelliTect.Coalesce.DotnetTool.csproj" -f net6.0 -o "${coalesceBuildDir}"`
        ],{ verbose: true }
    )
)

const coalesceKoGen = shell.task(`${dotnetCoalesce} ../../coalesce-ko.json `, { verbose: true });
coalesceKoGen.displayName = "coalesceKoGen";

exports.coalesceKo = gulp.series(
    // Build is required every time because the templates are compiled into the dll.
    // Sometimes the CoalesceExe folder doesn't get new DLLs and needs to have all files deleted.
    exports.coalesceBuild,
    coalesceKoGen,
    exports.copyTs
);

const coalesceVueGen = shell.task(`${dotnetCoalesce} ../../coalesce-vue.json `, { verbose: true });
coalesceVueGen.displayName = "coalesceVueGen";
exports.coalesceVue = gulp.parallel(
    gulp.series(
        exports.coalesceBuild,
        coalesceVueGen,
    )
    // TODO: This may not be needed anymore?
    //,gulp.series(
    //   shell.task("cd ../Coalesce.Web.Vue && npm i")
    //)
);

// gulp.task('coalesce:debug', ['coalesce:build'], shell.task(
//     `${dotnetCoalesce} --debug --verbosity debug`,
//     { verbose: true }
// ));