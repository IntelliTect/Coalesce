/// <binding AfterBuild='copy-files' ProjectOpened='default, copy-images' />
/*
This file in the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkId=518007
*/

var gulp = require("gulp"),
    //debug = require('gulp-debug'),
    flatten = require("gulp-flatten"),
    sourcemaps = require("gulp-sourcemaps"),
    sass = require("gulp-sass"),
    typescriptCompiler = require("gulp-typescript"),
    del = require("del"),
    path = require("path"),
    shell = require("gulp-shell"),
    gutil = require("gulp-util"),
    rename = require("gulp-rename"),
    fs = require("fs"),
    exec = require("child_process").exec;

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

function getFolders(dir) {
    return fs.readdirSync(dir)
        .filter(function (file) {
            return fs.statSync(path.join(dir, file)).isDirectory();
        });
}

gulp.task("clean-images",
    function () {
        return del(paths.img);
    });

gulp.task("copy-images",
    ["clean-images"],
    function () {
        gulp.src(paths.images + "**/*.{png,jpg,ico}")
            .pipe(gulp.dest(paths.img));
    });

gulp.task("img:watch",
    function () {
        gulp.watch(paths.images + "**/*.{png,jpg,ico}", ["copy-images"]);
    });

gulp.task("clean-lib",
    function (cb) {
        return del(paths.img);
    });

gulp.task("copy-lib",
    ["clean-lib"],
    function () {
        const packages = {
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

        for (var destinationDir in packages) {
            gulp.src(paths.npm + packages[destinationDir])
                .pipe(gulp.dest(paths.lib + destinationDir));
        }
    });

gulp.task("copy-files", ["copy-lib", "ts", "copy-js"]);

gulp.task("copy-js",
    function () {
        gulp.src(paths.scripts + "*.js")
            .pipe(gulp.dest(paths.js));
    });

gulp.task("js:watch",
    function () {
        gulp.watch(paths.scripts + "/*.js", ["copy-js"]);
    });


gulp.task("sass",
    function () {
        return gulp.src(paths.styles + "/*.scss")
            .pipe(sass({
                includePaths: [paths.npm + "bootstrap-sass/assets/stylesheets"]
            }).on("error", sass.logError))
            .pipe(gulp.dest(paths.css));
    });

gulp.task("sass:watch",
    function () {
        gulp.watch([paths.styles + "/*.scss"], ["sass"]);
    });


gulp.task("ts:local",
    function () {
        gulp.watch([paths.scripts + "/*.ts"], ["ts"]);
    });;

gulp.task('ts', function () {
    // compile the root generated code into an app.js file
    var rootAppJsProject = typescriptCompiler.createProject('tsconfig.json', { outFile: 'app.js' });
    var rootApp = gulp.src([paths.scripts + '/Generated/{Ko,ko}*.ts', paths.scripts + '/Partials/*.ts', '!' + paths.scripts + '/*.d.ts'])
        .pipe(sourcemaps.init())
        .pipe(rootAppJsProject());

    rootApp.dts
        .pipe(gulp.dest(paths.js));

    rootApp.js
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest(paths.js));

    // now compile the individual page files
    var individualFileTypescriptProject = typescriptCompiler.createProject('tsconfig.json');
    var individualTsResult = gulp.src([paths.scripts + '/*.ts', '!' + paths.scripts + '/{intellitect,Ko,ko}*.ts'])
        .pipe(sourcemaps.init())
        .pipe(individualFileTypescriptProject());

    individualTsResult.dts.pipe(gulp.dest(paths.js));

    individualTsResult.js
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest(paths.js));
});

gulp.task("copy-ts",
    ["ts"],
    function () {
        gulp.src(paths.scripts + "*.{ts,js.map}")
            .pipe(gulp.dest(paths.js));
    });

gulp.task("ts:watch",
    function () {
        gulp.watch([paths.scripts + "/**/*.ts"], ["ts:local"]);
        gulp.watch([paths.scripts + "/Partials/*.ts"], ["ts"]);
    });

gulp.task("watch",
    ["sass:watch", "ts:watch", "js:watch", "img:watch"],
    function () {
    });

gulp.task("default",
    ["copy-lib", "sass", "ts", "watch"],
    function () {
    });



var coalesceBuildDir = `${require('os').tmpdir()}/CoalesceExe`;

gulp.task('coalesce:cleanbuild', function (cb) {
    return del(coalesceBuildDir, { force: true });
});

gulp.task('coalesce:build', ['coalesce:cleanbuild'], shell.task([
        'dotnet restore --verbosity quiet "../IntelliTect.Coalesce.Cli"',
        `dotnet build "../IntelliTect.Coalesce.Cli/IntelliTect.Coalesce.Cli.csproj" -o "${coalesceBuildDir}" -f netcoreapp2.0`
    ],{ verbose: true }
));

// Build is required every time because the templates are compiled into the dll.
// Sometimes the CoalesceExe folder doesn't get new DLLs and needs to have all files deleted.
gulp.task('coalesce', ['coalesce:build'], shell.task
    ([
        `dotnet "${coalesceBuildDir}/dotnet-coalesce.dll" ` 
    //    `dotnet "${coalesceBuildDir}/dotnet-coalesce.dll" --verbosity debug ` 
    ],
    { verbose: true }
));


gulp.task('coalesce:debug', ['coalesce:build'], shell.task
    ([
        `dotnet "${coalesceBuildDir}/dotnet-coalesce.dll" --debug --verbosity debug`
    ],
    { verbose: true }
    ));