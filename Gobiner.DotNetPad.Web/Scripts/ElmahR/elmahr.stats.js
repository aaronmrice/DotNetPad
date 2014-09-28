(function () {

    elmahr.computations.stats = function (dataProvider, acc) {

        var r = {
            stats: acc.stats || [],
            types: acc.types || 0
        };

        dataProvider = dataProvider && dataProvider.dashboard && dataProvider.dashboard();
        if (!dataProvider) {
            return r;
        }

        var errors = dataProvider.errors(),
            historyFullErrorsStats = dataProvider.historyFullErrorsStats();

        $.extend(r, acc);

        var errorsByType =
            _.chain(errors)
             .groupBy(function (q) { return q.shortType; })
             .map(function (val, key) { return { key: key, value: val.length }; })
             .value();

        var fullErrorsByType =
            _.chain(historyFullErrorsStats)
             .groupBy(function (q) { return q.ShortType; })
             .map(function (val, key) {
                 return {
                     key: key,
                     value: _.chain(val)
                             .reduce(function (a, c) { return a + c.Count; }, 0)
                             .value()
                 };
             })
             .value();

        var combinedErrors =
            _.chain(errorsByType.concat(fullErrorsByType))
             .groupBy(function (q) { return q.key; })
             .map(function (val, key) {
                 return {
                     shortType: key,
                     value: _.chain(val)
                             .reduce(function (a, ce) {
                                 return a + ce.value;
                             }, 0).value()
                 };
             })
             .value();

        r.types =
        _.chain(combinedErrors)
         .groupBy(function (e) { return e.shortType; })
         .reduce(function (a) { return a + 1; }, 0)
         .value();

        _.chain(combinedErrors)
         .groupBy(function (q) { return q.shortType; })
         .map(function (val, key) {
             return {
                 key: key,
                 value: _.chain(val)
                         .reduce(function (a, ce) {
                             return a + ce.value;
                         }, 0).value()
             };
         })
         .sortBy(function (kvp) { return (10000 - kvp.value) + '/' + kvp.key; })
         .filter(function (kvp) { return kvp.key; })
         .each(function (kvp) { return r.stats.push(kvp); });

        return r;

    };

}());