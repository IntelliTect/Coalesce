/// <binding AfterBuild='copy-files' ProjectOpened='default' />
/*
This file in the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkId=518007
*/

var gulp = require('gulp'),
    flatten = require('gulp-flatten'),
    sourcemaps = require('gulp-sourcemaps'),
    sass = require('gulp-sass'),
    typescriptCompiler = require('gulp-typescript'),
    rimraf = require('rimraf'),
    path = require('path'),
    rename = require('gulp-rename'),
    fs = require('fs');

// Initialize directory paths.
var paths = {
    // Source Directory Paths
    bower: "./bower_components/",
    scripts: "Scripts/",
    styles: "Styles/",
    wwwroot: "./wwwroot/"
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
        "bootstrap-datetimepicker": "eonasdan-bootstrap-datetimepicker/build/**/*.{css,js}",
    }

    for (var destinationDir in bower) {
        gulp.src(paths.bower + bower[destinationDir])
          .pipe(gulp.dest(paths.lib + destinationDir));
    }
});

gulp.task("copy-files", ['copy-lib', 'copy-ts', 'copy-js']);

gulp.task("copy-js", function () {
    gulp.src(paths.scripts + "*.js")
        .pipe(gulp.dest(paths.js));
});

gulp.task('js:watch', function () {
    gulp.watch(paths.scripts + '/*.js', ['copy-js']);
});


gulp.task("sass", function () {
    gulp.src(paths.styles + '/*.scss')
    .pipe(sass().on('error', sass.logError))
    .pipe(gulp.dest(paths.css))
});

gulp.task('sass:watch', function () {
    gulp.watch([paths.styles + '/*.scss', 'Areas/**/Styles/*.scss'], ['sass']);
});

gulp.task('ts', function () {
    // compile the intellitect code into an intellitect.js file
    var intellitectTypescriptProject = typescriptCompiler.createProject('tsconfig.json', { outFile: 'intellitect.js' });
    var intellitectResult = gulp.src([paths.scripts + '/Coalesce/intellitect*.ts', '!' + paths.scripts + '/*.d.ts'])
    .pipe(sourcemaps.init())
    .pipe(typescriptCompiler(intellitectTypescriptProject));

    intellitectResult.dts
        .pipe(gulp.dest(paths.js));

    intellitectResult.js
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest(paths.js));

    // compile the root generated code into an app.js file
    var rootAppJsProject = typescriptCompiler.createProject('tsconfig.json', { outFile: 'app.js' });
    var rootApp = gulp.src([paths.scripts + '/Generated/{Ko,ko}*.ts', paths.scripts + '/Partials/{Ko,ko}*.ts', '!' + paths.scripts + '/*.d.ts'])
    .pipe(sourcemaps.init())
    .pipe(typescriptCompiler(rootAppJsProject));

    rootApp.dts
        .pipe(gulp.dest(paths.js));

    rootApp.js
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest(paths.js));

    // compile the area generated code into an app.js file
    var folders = getFolders('Areas');

    folders.map(function (folder) {
        var areaAppJsProject = typescriptCompiler.createProject('tsconfig.json', { outFile: 'app.js' });
        var areaApp = gulp.src([path.join('Areas', folder, 'Scripts', '**/{Ko,ko}*.ts'), path.join('!Areas', folder, '**/*.d.ts')])
		.pipe(sourcemaps.init())
		.pipe(typescriptCompiler(areaAppJsProject));

        areaApp.dts
			.pipe(gulp.dest(path.join(paths.wwwroot, folder, 'js')));

        areaApp.js
			.pipe(sourcemaps.write('.'))
			.pipe(gulp.dest(path.join(paths.wwwroot, folder, 'js')));
    });

    // now compile the individual page files
    var individualFileTypescriptProject = typescriptCompiler.createProject('tsconfig.json')
    var individualTsResult = gulp.src([paths.scripts + '/*.ts', '!' + paths.scripts + '/{intellitect,Ko,ko}*.ts'])
    .pipe(sourcemaps.init())
    .pipe(typescriptCompiler(individualFileTypescriptProject));

    individualTsResult.dts.pipe(gulp.dest(paths.js));

    individualTsResult.js
    .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest(paths.js));

    // now compile the areas individual page files
    individualFileTypescriptProject = typescriptCompiler.createProject('tsconfig.json')
    individualTsResult = gulp.src(['Areas/**/Scripts/*.ts', '!Areas/**/Scripts/**/{intellitect,Ko,ko}*.ts'])
    .pipe(sourcemaps.init())
    .pipe(typescriptCompiler(individualFileTypescriptProject));

    individualTsResult.dts.pipe(gulp.dest(paths.js));

    individualTsResult.js
    .pipe(sourcemaps.write('.'))
        .pipe(flatten({ includeParents: 1 }))
        .pipe(rename(function (path) {
            var originalPath = path.dirname;
            path.dirname += '/js';
        }))
        .pipe(gulp.dest(paths.wwwroot));
});

gulp.task("copy-ts", ['ts'], function () {
    gulp.src(paths.scripts + "*.{ts,js.map}")
        .pipe(gulp.dest(paths.js));
});

gulp.task('ts:watch', function () {
    gulp.watch([paths.scripts + '/**/*.ts', 'Areas/**/Scripts/**/*.ts'], ['ts']);
});

gulp.task('watch', ['sass:watch', 'ts:watch', 'js:watch'], function () {
});

gulp.task('default', ['copy-lib', 'sass', 'ts', 'watch'], function () {
});