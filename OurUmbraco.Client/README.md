# Our Umbraco - 2015 design

New design for our project.


## Getting started

To get startede simply clone this repository to you dev enviroment, and run a `npm install` to download the dependencies. Once compleated you can run `gulp` to start it up.

The workflow creates a `/build` directory whare all the compiled files go.
You can change this paths in the `gulpfile.js` file.


## Where do i put things

The `/src` directory is where your development files goes. A folder structure is included in the repo. It's fairly simple to get started with.

The `/src/css` directory is most likely to be completely removed since regular css files can go to the `/src/scss` directory. It's was intended that vendor css-files like *reset.css* or *animate.css* should go there and gulp automatically concat these files and puts them in the top of the compiled css file.

But, we have been thinking about removing it completely from the project and only stick with the `src/scss` directory.


## Constribution

If you discover a bug or have an enhancement, drop and issue and a pull-request to the master branch and we'll have a look at it.

It's important to remember that it's important that this script stay as small and effecient as possible.


## Authors

Rune Strand & Simon Busborg


