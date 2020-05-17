angular.module('myApp', ['dyFlipClock'])
    .run(function ($rootScope, $interval) {
        $rootScope.time = 0;
        $interval(function () {
            $rootScope.time += $("#interval").val();
        }, 1, 500);
    });