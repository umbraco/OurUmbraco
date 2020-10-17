'use strict';

const { task, src, dest, watch, series, parallel } = require('gulp');

const settings = {
  build: './build',
  source: 'src',
  umbraco: './../OurUmbraco.Site',
};

const jshint = require('gulp-jshint');
const stylish = require('jshint-stylish');
const gutil = require('gulp-util');
const uglify = require('gulp-uglify');
const concat = require('gulp-concat');
const sass = require('gulp-sass');
const autoprefixer = require('gulp-autoprefixer');
const cleanCss = require('gulp-clean-css');
const rename = require('gulp-rename');
const imageMin = require('gulp-imagemin');
const svgmin = require('gulp-svgmin');

// Lint js files
function lint() {
  return src(settings.source + '/js/*.js')
    .pipe(jshint())
    .pipe(jshint.reporter(stylish));
}

// Minify js files
function js() {
  return (
    src([settings.source + '/js/vendor/**/*.js', settings.source + '/js/*.js'])
      // Mangle is set to false by default.
      // By disabling mangle, Uglify won't rename
      // variables, functions etc.
      // This means that AngularJS apps will be
      // able to be minified without breaking.
      // If you aren't using angular, I recommend
      // enabling mangle, to obtain better compression.
      .pipe(uglify({ mangle: false }))
      .pipe(concat('app.min.js'))
      .on('error', gutil.log)
      .pipe(dest(settings.build + '/assets/js'))
      .pipe(dest(settings.umbraco + '/assets/js'))
  );
}

// Compile vendor css and scss
function css() {
  return src(settings.source + '/scss/*.scss')
    .pipe(sass())
    .on('error', gutil.log)
    .pipe(autoprefixer('last 1 version', 'ie 9', 'ios 7'))
    .pipe(rename({ suffix: '.min' }))
    .pipe(cleanCss({ level: 2 }))
    .pipe(dest(settings.build + '/assets/css'))
    .pipe(dest(settings.umbraco + '/assets/css'));
}

// Optimize images
function images() {
  return src([settings.source + '/images/*.png', settings.source + '/images/*.jpg', settings.source + '/images/*.gif'])
    .pipe(imageMin())
    .on('error', gutil.log)
    .pipe(dest(settings.build + '/assets/images'))
    .pipe(dest(settings.umbraco + '/assets/images'));
}

// SVG images
function svg() {
  return src(settings.source + '/images/*.svg')
    .pipe(svgmin())
    .on('error', gutil.log)
    .pipe(dest(settings.build + '/assets/images'))
    .pipe(dest(settings.umbraco + '/assets/images'));
}

function watchFiles() {
  watch([settings.source + '/js/vendor/**/*.js', settings.source + '/js/*.js'], parallel(lint, js));

  watch([settings.source + '/scss/**/*.scss', settings.source + '/css/**/*.css'], parallel(css));

  watch(settings.source + '/images/**', parallel(images, svg));
}

// Build task to used during normal development where you don't need the images to be updated.
exports.dev = parallel(lint, js, css);

// The same as above, but also minifying all the image files.
exports.build = series(parallel(lint, js, css, images, svg));

// Default task and watch files
exports.default = series(parallel(lint, js, css, images, svg), watchFiles);
