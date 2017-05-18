/// <binding AfterBuild='copy-files' ProjectOpened='default, copy-images' />
/*
This file in the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkId=518007
*/

var gulp = require('gulp'),
    //debug = require('gulp-debug'),
    flatten = require('gulp-flatten'),
    sourcemaps = require('gulp-sourcemaps'),
    sass = require('gulp-sass'),
    typescriptCompiler = require('gulp-typescript'),
    rimraf = require('rimraf'),
    path = require('path'),
    shell = require('gulp-shell'),
    gutil = require('gulp-util'),
    rename = require('gulp-rename'),
    fs = require('fs'),
    exec = require('child_process').exec;

// Initialize directory paths.
var paths = {
    // Source Directory Paths
    bower: "./bower_components/",
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

gulp.task('clean-images', function (cb) {
    rimraf(paths.img, cb);
});

gulp.task("copy-images", ['clean-images'], function () {
    gulp.src(paths.images + '**/*.{png,jpg,ico}')
        .pipe(gulp.dest(paths.img));
});

gulp.task('img:watch', function () {
    gulp.watch(paths.images + '**/*.{png,jpg,ico}', ['copy-images']);
});

gulp.task("clean-lib", function (cb) {
    rimraf(paths.lib, cb);
});

gulp.task("copy-lib", ['clean-lib'], function () {
    var bower = {
        "bootstrap": "bootstrap/dist/**/bootstrap*.{js,map,css}",
        "bootstrap/fonts": "bootstrap/fonts/*.{,eot,svg,ttf,woff,woff2}",
        "jquery": "jquery/dist/jquery*.{js,map}",
        "font-awesome": "components-font-awesome/**/*.{css,otf,eot,svg,ttf,woff,woff2}",
        "simple-line-icons": "simple-line-icons/**/*.{css,otf,eot,svg,ttf,woff,woff2}",
        "uniform": "jquery.uniform/**/{jquery.uniform.js,uniform.default.css}",
        "bootstrap-switch": "bootstrap-switch/dist/**/*.{js,css}",
        "bootstrap-daterangepicker": "bootstrap-daterangepicker/*.{js,css}",
        "bootstrap-hover-dropdown": "bootstrap-hover-dropdown/bootstrap-hover-dropdown*.js",
        "counter-up": "counter-up/*.js",
        "waypoints": "waypoints/lib/jquery.waypoints.js",
        "morris": "morris.js/morris.{css,js}",
        "fullcalendar": "fullcalendar/dist/fullcalendar.{css,js}",
        "jqvmap": "jqvmap/jqvmap/**/j*.{css,js}",
        "moment": "moment/moment.js",
        "jquery-slimscroll": "jquery.slimscroll/jquery.*.js",
        "knockout": "knockout/dist/*.js",
        "knockout-validation": "knockout-validation/dist/*.js",
        "select2": "select2/dist/**/*.{css,js}",
        "bootstrap-datetimepicker": "eonasdan-bootstrap-datetimepicker/build/**/*.{css,js}"
    };

    for (var destinationDir in bower) {
        gulp.src(paths.bower + bower[destinationDir])
          .pipe(gulp.dest(paths.lib + destinationDir));
    }
});

gulp.task("copy-files", ['copy-lib', 'ts', 'copy-js']);

gulp.task("copy-js", function () {
    gulp.src(paths.scripts + "*.js")
        .pipe(gulp.dest(paths.js));
});

gulp.task('js:watch', function () {
    gulp.watch(paths.scripts + '/*.js', ['copy-js']);
});


gulp.task("sass", function () {
    //gulp.src([paths.styles + '/*.scss', 'Areas/*/Styles/*.scss'])
    // get the files from the root
    gulp.src(paths.styles + '/*.scss')
    .pipe(sass().on('error', sass.logError))
    .pipe(gulp.dest(paths.css));

    //// get the files from the areas
    //gulp.src('Areas/**/Styles/*.scss')
    //.pipe(sass().on('error', sass.logError))
    //    .pipe(flatten({ includeParents: 1 }))
    //    .pipe(rename(function(path) {
    //        var originalPath = path.dirname;
    //        path.dirname += '/css';
    //}))
    //.pipe(gulp.dest(paths.wwwroot));
});

gulp.task('sass:watch', function () {
    gulp.watch([paths.styles + '/*.scss'], ['sass']);
    //gulp.watch([paths.styles + '/*.scss', 'Areas/**/Styles/*.scss'], ['sass']);
});


gulp.task('ts:local', function () {
    // now compile the individual page files
    var individualFileTypescriptProject = typescriptCompiler.createProject('tsconfig.json');
    var individualTsResult = gulp.src([paths.scripts + '/*.ts', '!' + paths.scripts + '/{coalesce,Ko,ko}*.ts'])
        .pipe(sourcemaps.init())
        .pipe(individualFileTypescriptProject());

    individualTsResult.dts.pipe(gulp.dest(paths.js));

    individualTsResult.js
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest(paths.js));
});

gulp.task('ts', ['ts:local'], function () {
    // compile the root generated code into an app.js file
    var rootAppJsProject = typescriptCompiler.createProject('tsconfig.json', { outFile: 'app.js' });
    var rootApp = gulp.src([paths.scripts + 'viewmodels.generated.d.ts'])
        .pipe(sourcemaps.init())
        .pipe(rootAppJsProject());

    rootApp.dts
        .pipe(gulp.dest(paths.js));

    rootApp.js
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest(paths.js));
});

gulp.task("copy-ts", ['ts'], function () {
    gulp.src(paths.scripts + "*.{ts,js.map}")
        .pipe(gulp.dest(paths.js));
});

gulp.task('ts:watch', function () {
    gulp.watch([paths.scripts + '/**/*.ts'], ['ts:local']);
    gulp.watch([paths.scripts + '/Partials/*.ts'], ['ts']);
});

gulp.task('watch', ['sass:watch', 'ts:watch', 'js:watch', 'img:watch'], function () {
});

gulp.task('default', ['copy-lib', 'sass', 'ts', 'watch'], function () {
});

/*

var componentModelVersion = "1.1.0";
var codeGeneratorsMvcVersion = componentModelVersion;
var nlogExtensionsVersion = "1.1.0";

gulp.task('nuget:publish:ComponentModel',
    shell.task(['bower_components\\eonasdan-bootstrap-datetimepicker\\src\\nuget\\nuget ' +
        'push ' +
        '..\\..\\artifacts\\bin\\IntelliTect.Coalesce\\debug\\IntelliTect.Coalesce.' + componentModelVersion + '.nupkg ' +
        '536300da-5e23-433c-8f45-f84e9a225b4b ' +
        '-Source https://www.myget.org/F/intellitect-public/api/v2/package'])
);

gulp.task('nuget:publish:CodeGeneratorsMvc',
    shell.task(['bower_components\\eonasdan-bootstrap-datetimepicker\\src\\nuget\\nuget ' +
        'push ' +
        '..\\..\\artifacts\\bin\\IntelliTect.Coalesce.CodeGeneration\\debug\\IntelliTect.Coalesce.CodeGeneration.' + codeGeneratorsMvcVersion + '.nupkg ' +
        '536300da-5e23-433c-8f45-f84e9a225b4b ' +
        '-Source https://www.myget.org/F/intellitect-public/api/v2/package'])
);

gulp.task('nuget:publish:NLogExtensions',
    shell.task(['bower_components\\eonasdan-bootstrap-datetimepicker\\src\\nuget\\nuget ' +
        'push ' +
        '..\\..\\artifacts\\bin\\IntelliTect.NLog.Extensions\\debug\\IntelliTect.NLog.Extensions.' + nlogExtensionsVersion + '.nupkg ' +
        '536300da-5e23-433c-8f45-f84e9a225b4b ' +
        '-Source https://www.myget.org/F/intellitect-public/api/v2/package'])
);

gulp.task('nuget:publish', ['nuget:publish:ComponentModel', 'nuget:publish:CodeGeneratorsMvc', 'nuget:publish:NLogExtensions']);

*/


gulp.task('coalesce:build', shell.task([
        //'if exist "%temp%/CoalesceExe" rmdir "%temp%/CoalesceExe" /s /q',
        'dotnet msbuild /t:restore /v:q "../IntelliTect.Coalesce.Cli"',
        'dotnet build "../IntelliTect.Coalesce.Cli/IntelliTect.Coalesce.Cli.csproj" -o %temp%/CoalesceExe -f net46'
    ],{ verbose: true }
));

// Build is required every time because the templates are compiled into the dll.
// Sometimes the CoalesceExe folder doesn't get new DLLs and needs to have all files deleted.
gulp.task('coalesce', ['coalesce:build'], shell.task
    ([
        '"%temp%/CoalesceExe/IntelliTect.Coalesce.Cli.exe" ' +
        '-dc AppDbContext -dp ..\\Coalesce.Domain -wp .\\ -filesOnly true -ns Coalesce.Web'
    ],
    { verbose: true }
));