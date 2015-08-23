# Our Umbraco - 2015 design

New design for the Our project.


## Getting started

To get started simply clone this repository to you dev environment, and run `npm install` to download the dependencies. Once completed, you can run `gulp` to start it up.

The workflow creates a `/build` directory where all the compiled files go.
You can change this path in the `gulpfile.js` file.


## Where do I put things?

The `/src` directory is where your development files go. A folder structure is included in the repo. It's fairly simple to get started with.

The `/src/css` directory is most likely to be completely removed since regular CSS files can go to the `/src/scss` directory. It was intended that vendor CSS-files like *reset.css* or *animate.css* should go there and gulp would automatically concatenate these files and put them at the top of the compiled CSS file.

But we have been thinking about removing it completely from the project and only stick with the `src/scss` directory.


## Contribution

If you discover a bug or have an enhancement, drop and [issue](http://issues.umbraco.org/issues) and a pull-request to the `master` branch and we'll have a look at it.

It's important to remember that this script stays as small and efficient as possible.


## Authors

Rune Strand & Simon Busborg


