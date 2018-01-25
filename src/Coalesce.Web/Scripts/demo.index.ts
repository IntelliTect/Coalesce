/// <reference path="viewmodels.generated.d.ts" />

module Demo {
    class DemoViewModel {
        allOpenCases = new ListViewModels.CaseList();

        weatherService = new Services.WeatherServiceClient();

        constructor() {
            this.allOpenCases.pageSize(100);
            this.allOpenCases.dataSource = new this.allOpenCases.dataSources.AllOpenCases();
            this.allOpenCases.dataSource.subscribe(this.allOpenCases);
            this.allOpenCases.load();

            this.weatherService.getWeather.invoke();
        }
    }

    $(function () {
        ko.applyBindings(new DemoViewModel());
    });
}