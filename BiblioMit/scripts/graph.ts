//CHART
am4core.ready(function () {
    //DATES
    $('.input-daterange').datepicker({
        inputs: $('.actual_range'),
        format: 'yyyy-MM-dd',
        language: 'es'
    });

    am4core.useTheme(am4themes_kelly);
    var chart4: any = am4core.create("chartdiv", am4charts.XYChart);
    var data = [];
    var value = 50;
    for (var i = 0; i < 300; i++) {
        var date = new Date();
        date.setHours(0, 0, 0, 0);
        date.setDate(i);
        value -= Math.round((Math.random() < 0.5 ? 1 : -1) * Math.random() * 10);
        data.push({ date: date, value: value });
    }

    chart4.data = data;

    // Create axes
    var dateAxis = chart4.xAxes.push(new am4charts.DateAxis());
    dateAxis.renderer.minGridDistance = 60;

    var valueAxis = chart4.yAxes.push(new am4charts.ValueAxis());

    // Create series
    var series = chart4.series.push(new am4charts.LineSeries());
    series.dataFields.valueY = "value";
    series.dataFields.dateX = "date";
    series.tooltipText = "{value}"

    series.tooltip.pointerOrientation = "vertical";

    chart4.cursor = new am4charts.XYCursor();
    chart4.cursor.snapToSeries = series;
    chart4.cursor.xAxis = dateAxis;

    //chart.scrollbarY = new am4core.Scrollbar();
    chart4.scrollbarX = new am4core.Scrollbar();
});