define(["require", "exports", 'bootstrap'], function (require, exports) {
    function configure(aurelia) {
        aurelia.use
            .standardConfiguration()
            .developmentLogging();
        aurelia.start().then(function (a) { return a.setRoot(); });
    }
    exports.configure = configure;
});
//# sourceMappingURL=main.js.map