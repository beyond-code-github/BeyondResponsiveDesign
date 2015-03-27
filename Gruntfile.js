module.exports = function(grunt) {

  grunt.initConfig({
    appcache: {
      options: {
        basePath: 'BeyondResponsiveDesign'
      },
      all: {
        dest: 'BeyondResponsiveDesign/cache.manifest',
        cache: {
          patterns: ['BeyondResponsiveDesign/**/*'],
          literals: [
            'styles/fonts/opensans_regular_macroman/OpenSans-Regular-webfont.eot',
            'styles/fonts/opensans_regular_macroman/OpenSans-Regular-webfont.eot?iefix',
            'styles/fonts/opensans_regular_macroman/OpenSans-Regular-webfont.svg#webfont',
            'styles/fonts/opensans_regular_macroman/OpenSans-Regular-webfont.ttf',
            'styles/fonts/opensans_regular_macroman/OpenSans-Regular-webfont.woff',
            'styles/fonts/fontawesome-webfont.eot?v=4.1.0',
            'styles/fonts/fontawesome-webfont.eot?#iefix&v=4.1.0',
            'styles/fonts/fontawesome-webfont.svg?v=4.1.0#fontawesomeregular',
            'styles/fonts/fontawesome-webfont.ttf?v=4.1.0',
            'styles/fonts/fontawesome-webfont.woff?v=4.1.0']
        },
        network: '*',
        fallback: '/ /offline.html'
      }
    }
  });

  grunt.loadNpmTasks('grunt-appcache');

  grunt.registerTask('default', ['appcache']);
};