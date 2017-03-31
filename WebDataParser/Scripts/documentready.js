$(document).ready(function () {

	var data = [
		{
		    label: "Tilt",
		    data: TiltData 
		}, 
		{
		    label: "Throw",
		    data: ThrowData 
		}, 
		{
		    label: "Swipe",
		    data: SwipeData 
		}, 
		{
		    label: "Pinch",
		    data: PinchData 
		}
	];

    var timeData = [
		{
		    label: "Tilt",
		    data: TimeTiltData 
		}, 
		{
		    label: "Throw",
		    data: TimeThrowData 
		}, 
		{
		    label: "Swipe",
		    data: TimeSwipeData 
		}, 
		{
		    label: "Pinch",
		    data: TimePinchData 
		}
    ];
	
    $.plot("#hitpercentage", data,
		{
		    yaxis: {
		        ticks: 10,
		        axisLabel: "%",
		        axisLabelFontFamily: 'Times New Roman',
		        max: 100,
		        tickDecimals: 0
		    }, 
		    xaxis: {
		        mode: "categories",
		        axisLabel: "Target",
		        axisLabelFontFamily: 'Times New Roman',
		        tickDecimals: 0
		    },
		    grid: {
		        borderWidth: 1
		    },
		    legend: { 
		        position: "se",
		        backgroundOpacity: 0.5,
		        container: "#HitPercentlegend"
		    }	
		}
	);

    $.plot("#timepertarget", timeData, {
        
        yaxis: {
            axisLabel: "Seconds",
            axisLabelFontFamily: 'Times New Roman'
        },
        xaxis: {
            mode: "categories",
            axisLabel: "Target",
            axisLabelFontFamily: 'Times New Roman',
            tickLength: 0
        },
        grid: {
            borderWidth: 1
        },
        legend: {
            position: "se",
            backgroundOpacity: 0.5,
            container: "#TimeLegend"
        }
    });
});	