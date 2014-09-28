(function () {

    //#region Patching console for IE<10
    
    if (!window.console) window.console = {};
    if (!window.console.log) window.console.log = function () { };
    
    //#endregion
    
    //#region String

    String.prototype.format = function () {
        var args = arguments;
        return this.replace(/%((%)|[0-9]+)/g, function (m, num, pct) {
            return pct || (num = parseInt(num, 10), num >= 0 && num < args.length ? args[num] : m);
        });
    };
    String.prototype.trimMillisecondsFromIsoDate = function () {
        //this solves a bug with IE
        return new Date(Date.parse(this.substring(0, 23) + (this.indexOf('+') >= 0 ? this.substring(this.indexOf('+')) : '')));
    };
    String.prototype.newGuid = function () {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    };

    //#endregion


    //#region Date

    var dateMarkers = {
        d: ['getDate', function (v) { return ("0" + v).substr(-2, 2); }],
        m: ['getMonth', function (v) { return ("0" + v).substr(-2, 2); }],
        n: ['getMonth', function (v) {
            var mthNames = ["jan", "feb", "mar", "apr", "may", "jun", "jul", "aug", "sep", "oct", "nov", "dec"];
            return mthNames[v];
        }],
        w: ['getDay', function (v) {
            var dayNames = ["sun", "mon", "tue", "wed", "thu", "fri", "sat"];
            return dayNames[v];
        }],
        y: ['getFullYear'],
        H: ['getHours', function (v) { return ("0" + v).substr(-2, 2); }],
        M: ['getMinutes', function (v) { return ("0" + v).substr(-2, 2); }],
        S: ['getSeconds', function (v) { return ("0" + v).substr(-2, 2); }],
        i: ['toISOString', null]
    };

    var format = function (date, fmt) {
        var dateTxt = fmt.replace(/%(.)/g, function (m, p) {
            var rv = date[(dateMarkers[p])[0]]();

            if (dateMarkers[p][1] != null)
                rv = dateMarkers[p][1](rv);

            return rv;
        });

        return dateTxt;
    };

    Date.prototype.format = function (fmt) {
        return format(this, fmt);
    };

    //#endregion


    //#region Browser focus

    var hidden, change, vis = {
        hidden: "visibilitychange",
        mozHidden: "mozvisibilitychange",
        webkitHidden: "webkitvisibilitychange",
        msHidden: "msvisibilitychange",
        oHidden: "ovisibilitychange" // not currently supported
    };
    for (hidden in vis) {
        if (vis.hasOwnProperty(hidden) && hidden in document) {
            change = vis[hidden];
            break;
        }
    }
    if (change)
        document.addEventListener(change, onchange);
    else if ($.browser.version.indexOf('Internet Explorer') >= 0) // l 9 and lower
        document.onfocusin = document.onfocusout = onchange;
    else
        window.onfocus = window.onblur = onchange;
    function onchange(evt) {
        evt = evt || window.event;
        elmahr.setBrowserVisible(evt.type !== "blur" && evt.type !== "focusout"
                                 ? !this[hidden]
                                 : false);
    }

    //#endregion


    //#region elmahr global module

    this.elmahr = (function () {

        //#region Local vars

        var elmahrConnector,    //initialization happens later
            hookPlugins = function (plugins) {
                if ($.isEmptyObject(plugins))
                    return;
                for (var i = 0; i < plugins.length; i++) {
                    var plugin = plugins[i], name = plugin.Key;
                    var file = plugin.Value;
                    var selector =
                        file.Key.toLowerCase() === 'script' || file.Key.toLowerCase() === 'css'
                            ? 'head'
                            : '#' + file.Key;
                    try {
                        $(selector).append(file.Value);
                    } catch (e) {
                        elmahr.onLog("Error loading ElmahR plugin %0: %1 ".format(name, e));
                    }
                }
            },
            externalBuilder = [{
                startup: function () {
                },
                onLog: function (message) {
                    if (console) {
                        console.log(message);
                    }
                }
            }],
            sm = function (message) {
                var $body = $('#messageslog'), m = $('<p/>').text(message);
                m.prependTo($body);
            },
            dataProvider = {},
            computationsReceiver = {};

        //#endregion


        //#region elmahr object

        return {

            registerBuilder: function (builder) {
                externalBuilder.push(builder);
            },

            computations: {},

            plugins: {},

            config: {
                root: '/',
                imagesPath: 'content/images/',
                title: 'ElmahR Dashboard',
                appBoxIdPrefix: 'app',
                messageFeedPaused: 'This feed is paused, press here to receive errors',
                messageFeedRunning: 'This feed is running, press here to pause it'
            },

            // methods

            start: function () {
                this.statusReady();
            },

            toggleLog: function () {
                elmahr.clearLog();
                elmahr.onLog('Subscribing to log...');
                elmahrConnector.toggleLog()
                               .done(function () {
                                   elmahr.onLog('...log ready!');
                               });
            },

            startup: function () {
                if (externalBuilder) {
                    externalBuilder.startup();
                }
            },

            statsRefresher: function (computation) {

                computation = computation || elmahr.computations;

                var x = {};

                if (typeof computation === 'string') {
                    try {
                        $.extend(x, elmahr.computations[computation](dataProvider, x));
                    } catch (e) {
                        elmahr.onLog("Error loading ElmahR computation %0: %1 ".format(computation, e));
                    }
                } else {
                    for (var k in computation) {
                        try {
                            $.extend(x, computation[k](dataProvider, x));
                        } catch (e) {
                            elmahr.onLog("Error loading ElmahR computation %0: %1 ".format(k, e));
                        }
                    }
                }
                elmahr.statsReceiver(x);
            },

            statsReceiver: function (x) {
                for (var c in computationsReceiver) {
                    if (computationsReceiver.hasOwnProperty(c)) {
                        try {
                            computationsReceiver[c](x);
                        } catch (e) {
                            elmahr.onLog("Error processing data with receiver %0: %1 ".format(c, e));
                        }
                    }
                }
            },

            registerDataProvider: function (provider) {
                $.extend(dataProvider, provider);
            },

            registerComputationsReceiver: function (receiver) {
                $.extend(computationsReceiver, receiver);
            },

            // init

            init: function () {

                var buildConnector = function () {
                    elmahrConnector = $.hubConnection(elmahr.config.signalrRoot).createHubProxy('elmahr');
                    $.extend(elmahrConnector, {
                        toggleLog: function () {
                            return this.invoke('toggleLog');
                        },
                        connect: function () {
                            return this.invoke('connect');
                        },
                        askForApplications: function () {
                            return this.invoke('askForApplications');
                        },
                        retrieveHistoricalErrors: function () {
                            return this.invoke('retrieveHistoricalErrors');
                        },
                        retrieveFullErrorsStats: function () {
                            return this.invoke('retrieveFullErrorsStats');
                        }
                    });

                    var chainer =
                        function (source, extender, that) {
                            var target = {};
                            for (var p in extender) {
                                if (p === 'callbacks' && source.callbacks) {
                                    var chained = { callbacks: chainer(source[p], extender[p], that) };
                                    $.extend(target, chained);
                                } else if (!source.hasOwnProperty(p)) {
                                    target[p] = extender[p];
                                } else {
                                    if (typeof source[p] === 'function') {
                                        (function (prev, next, cb, it) {
                                            target[cb] = function () {
                                                if (next.replacer) {
                                                    return next.replacer.apply(it, arguments);
                                                }
                                                prev.apply(it, arguments);
                                                return next.apply(it, arguments);
                                            };
                                        })(source[p], extender[p], p, that);
                                    } else {
                                        target[p] = extender[p];
                                    }
                                }
                            }
                            return target;
                        },
                        callbacks = {
                            advertiseApplications: function (apps) {
                                for (i = 0, l = apps.length; i < l; i++) {
                                    var s = 'Advertising application: %0, id: %1'.format(apps[i].ApplicationName, apps[i].SourceId);
                                    elmahr.printMessage(s);
                                }
                            },

                            notifyErrors: function (envelopes, append, startupErrors) {
                                for (i = 0, l = envelopes.length; i < l; i++) {
                                    var envelope = envelopes[i],
                                        s = '%2: Error from application %0 (%1):\r\n%3'.format(
                                            envelope.ApplicationName,
                                            envelope.SourceId,
                                            envelope.Error.Time.trimMillisecondsFromIsoDate().format("%d-%n-%y %H:%M:%S"),
                                            envelope.Error.Message);
                                    elmahr.onLog(s);
                                    elmahr.printMessage(s);
                                }
                            },

                            notifyFullError: function (error) {
                            },

                            notifyRememberMe: function (remember) {
                            },

                            notifyErrorsResume: function (sourceId, resume) {
                            },

                            notifyFullErrorsStats: function (errorsStats) {
                            },

                            log: function (message) {
                                elmahr.onLog(message);
                            }
                        };

                    var builder = {
                        callbacks: callbacks,

                        //default functions meant to be overridden

                        onSelectError: function () { },
                        onNewError: function () { },
                        onRememberMe: function () { },
                        hideAskForMore: function () { },
                        statusConnecting: function () { sm('connecting...'); },
                        statusConnected: function () { sm('connected!'); },
                        statusSendingCommand: function () { sm('sending commands...'); },
                        statusStartingHubs: function () { sm('starting hubs...'); },
                        statusReady: function () { sm('ready!'); },
                        statusRefreshingStats: function () { sm('refreshing stats...'); },
                        statusReceiving: function () { sm('receiving...'); },
                        startTransfer: function () { sm('start transfer'); },
                        stopTransfer: function () { sm('stop transfer'); },
                        printMessage: sm,
                        getSortedApplications: function () { },
                        toggleBinLoading: function () { },
                        onGetErrorsResumeDone: function () { },
                        onLog: function () { },
                        clearLog: function () { },
                        setBrowserVisible: function () { }
                    };
                    for (var i = 0, l = externalBuilder.length; i < l; i++) {
                        var eb = externalBuilder[i],
                            ebc = eb && (typeof eb === 'function' && eb(elmahrConnector, elmahr && elmahr.data) || eb),
                            ex = chainer(builder, ebc, elmahr);
                        $.extend(builder, ex);
                    }

                    // building elmahr 
                    $.extend(elmahr, builder);

                    // proxy hub method defs
                    callbacks = builder && builder.callbacks || callbacks;
                    for (var m in callbacks) {
                        (function (method) {
                            if (callbacks.hasOwnProperty(method)) {
                                elmahrConnector.on(method, function () {
                                    callbacks[method].apply(elmahrConnector, arguments);
                                });
                            }
                        })(m);
                    }

                    // now let's startup things!

                    elmahrConnector.connection
                        .starting(function () {
                            elmahr.printMessage('SignalR endpoint at: ' + elmahr.config.signalrRoot, 'Info');
                            elmahr.printMessage('Welcome to ' + elmahr.config.title, 'Info');
                        })
                        .disconnected(function () {
                            elmahr.printMessage('Hmm, disconnected...', 'Warning');
                            setTimeout(function () {
                                buildConnector();
                                elmahrConnector.connection.start()
                                    .done(function () {
                                        elmahrConnector.connect();
                                    });
                            }, 2000);
                        })
                        .reconnected(function () {
                            elmahr.printMessage('Reconnected!', 'Info');
                        })
                        .error(function (e) {
                            elmahr.printMessage('Oops, an error has occurred... but I will retry!', 'Critical');
                        });
                };

                buildConnector();

                $.connection(elmahr.config.root + 'elmahr/commands')
                    .start()
                    .done(function (connection) {
                        elmahr.statusSendingCommand();

                        connection.received(function (answer) {
                            switch (answer.Selector) {
                                case 'startup':
                                    hookPlugins(answer.Answer);

                                    elmahr.statusStartingHubs();
                                    elmahrConnector.connection.start()
                                        .done(function () {

                                            elmahrConnector.connection.stateChanged(function (ch) {
                                                if (ch.newState === $.signalR.connectionState.connected) {
                                                    elmahr.printMessage('Connected!', 'Info');
                                                }
                                            });

                                            elmahr.statusConnecting();
                                            elmahrConnector.connect().done(function () {
                                                elmahr.start();
                                                elmahrConnector.askForApplications().done(function () {
                                                    elmahrConnector.retrieveHistoricalErrors().done(function () {
                                                        elmahrConnector.retrieveFullErrorsStats().done(function () {
                                                            elmahr.statusConnected();
                                                        });
                                                    });
                                                });
                                            });
                                        });

                                    break;
                            }
                        });

                        connection.send('startup');
                    });

            }

        };

        //#endregion

    })();

    //#endregion

}());

