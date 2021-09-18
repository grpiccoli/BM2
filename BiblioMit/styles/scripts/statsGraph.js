var chart;
am4core.ready(function () {
    am4core.useTheme(am4themes_animated);
    chart = am4core.create("chartdiv", am4charts.PieChart);
    var pieSeries = chart.series.push(new am4charts.PieSeries());
    pieSeries.dataFields.value = "resultados";
    pieSeries.dataFields.category = "repositorio";
    chart.innerRadius = am4core.percent(30);
    pieSeries.dataFields.colorField = "color";
    // Put a thick white border around each Slice
    pieSeries.slices.template.stroke = am4core.color("#fff");
    pieSeries.slices.template.strokeWidth = 2;
    pieSeries.slices.template.strokeOpacity = 1;
    pieSeries.slices.template
        // change the cursor on hover to make it apparent the object can be interacted with
        .cursorOverStyle = [
            {
                "property": "cursor",
                "value": "pointer"
            }
        ];
    pieSeries.alignLabels = false;
    pieSeries.labels.template.bent = true;
    pieSeries.labels.template.radius = 3;
    pieSeries.labels.template.padding(0, 0, 0, 0);

    pieSeries.ticks.template.disabled = true;
    // Create a base filter effect (as if it's not there) for the hover to return to
    var shadow = pieSeries.slices.template.filters.push(new am4core.DropShadowFilter);
    shadow.opacity = 0;
    // Create hover state
    var hoverState = pieSeries.slices.template.states.getKey("hover"); // normally we have to create the hover state, in this case it already exists
    // Slightly shift the shadow and make it more prominent on hover
    var hoverShadow = hoverState.filters.push(new am4core.DropShadowFilter);
    hoverShadow.opacity = 0.7;
    hoverShadow.blur = 5;
    // Add a legend
    legend = new am4charts.Legend();
    chart.data = chartData;

    chart.balloonText = "[[title]]<br><span style='font-size:10px'><b>[[value]]</b> ([[percents]]%)</span>";

    legend.divId = "legenddiv";
    legend.position = "right";
    chart.legend = legend;
});

//function selectData(i) {
//    chart.dataProvider = chartData[i];
//    chart.validateData();
//    $("#chartdiv tspan").html(titles[i]);
//}