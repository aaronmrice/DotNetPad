(function () {

    //#region elmahrBuilder global module

    elmahr.registerBuilder(function (elmahrConnector, data) {

        var play = 'play',
            pause = 'pause',
            show = 'show',
            hide = 'hide';

        //#region View models

        function ErrorViewModel(envelope, sourceId, infoUrl) {

            if (!(this instanceof ErrorViewModel)) {
                return new ErrorViewModel(envelope, sourceId, infoUrl);
            }

            var self = this;

            var time = envelope.Error.Time.trimMillisecondsFromIsoDate().toLocaleString(),
                e = envelope.Error;

            self.id = e.Id;
            self.applicationName = envelope.ApplicationName;
            self.sourceId = sourceId;
            self.isFull = e.IsFull;
            self.infoUrl = infoUrl;
            self.message = e.Message;
            self.time = time;
            self.rawTime = envelope.Error.Time;
            self.isoTime = e.IsoTime;
            self.host = e.Host;
            self.type = e.Type;
            self.shortType = e.ShortType;
            self.source = e.Source;
            self.detail = e.Detail;
            self.user = e.User;
            self.statusCode = e.StatusCode;
            self.webHostHtmlMessage = e.WebHostHtmlMessage;
            self.hasYsod = e.HasYsod;
            self.url = elmahr.config.root + e.Url;
            self.browserSupportUrl = elmahr.config.imagesPath + e.BrowserSupportUrl;
            self.reconnectClass = envelope.Class !== null;

            self.serverVariables = [];
            self.form = [];
            self.cookies = [];

            for (var sv in e.ServerVariables) {
                self.serverVariables.push({
                    'key': sv,
                    'value': e.ServerVariables[sv]
                });
            }

            for (var f in e.Form) {
                self.form.push({
                    'key': f,
                    'value': e.Form[f]
                });
            }

            for (var c in e.Cookies) {
                self.cookies.push({
                    'key': c,
                    'value': e.Cookies[c]
                });
            }

            self.popup = function (target) {
                if (target.isFull) {
                    selected(target);
                    elmahr.onSelectError(target);
                } else
                    elmahr.selectTarget(target.id, target.sourceId);
            };
        }

        function ErrorBarrierViewModel(application, when, offset, countBefore, countAfter) {

            if (!(this instanceof ErrorBarrierViewModel)) {
                return new ErrorBarrierViewModel(application, when, offset, countBefore, countAfter);
            }

            var self = this;

            self.application = application;
            self.when = when.trimMillisecondsFromIsoDate().format('%d-%n-%y');
            self.offset = offset ? -1 * offset + ' days before latest day with errors' : 'latest day with errors';
            self.countBefore = countBefore;
            self.countAfter = countAfter;

            var clear = function (target, event, action) {
                if (application.deletor())
                    return;
                elmahr.toggleBinLoading($(event.target), true);
                action(function () {
                    elmahr.toggleBinLoading($(event.target), false);
                });
            };

            self.clearErrorsBefore = function (target, event) {
                clear(target, event, function (post) { elmahr.askToClearErrorsBefore(application.sourceId, when, post); });
            };
            self.clearErrorsAfter = function (target, event) {
                clear(target, event, function (post) { elmahr.askToClearErrorsAfter(application.sourceId, when, post); });
            };
        }

        function ErrorTypeModel(key, value) {

            if (!(this instanceof ErrorTypeModel)) {
                return new ErrorTypeModel(key, value);
            }

            var self = this;

            self.key = key;
            self.value = value;

            self.feedTitle = function () {
                return elmahr.isErrorTypeBlocked(key)
                    ? 'This error type is paused, press here to resume it'
                    : 'This error type is active, press here to pause it';
            };

            self.toggleErrorType = function (target, e) {
                var type = target.key;

                var t = $(e.currentTarget);
                var c = t.attr('class');
                var n = c === play ? pause : play;

                t.switchClass(c, n, 0);

                elmahr.toggleErrorType(type, n);

                t.attr('title', self.feedTitle());
            };

            this.errorClass = ko.computed(function () {
                return elmahr.isErrorTypeBlocked(key) ? pause : play;
            }, this);

            self.titlePlayPause = ko.computed(self.feedTitle, this);
        }

        function ApplicationViewModel(applicationName, infoUrl, sourceId, hasTestExceptionUrl, boxClass, index) {

            if (!(this instanceof ApplicationViewModel)) {
                return new ApplicationViewModel(applicationName, infoUrl, sourceId, hasTestExceptionUrl, boxClass, index);
            }

            var self = this;

            self.applicationName = applicationName;
            self.infoUrl = infoUrl;
            self.sourceId = sourceId;
            self.index = index;
            self.hasTestExceptionUrl = hasTestExceptionUrl;
            self.boxClassBase = boxClass
                ? boxClass
                : 'boxColor' + (index % 3);

            //observables

            self.boxClass = ko.observable(self.boxClassBase + ' ' + (elmahr.isApplicationBlocked(sourceId) ? 'grayed' : ''));
            self.errors = ko.observableArray([]);
            self.lastError = ko.observable(null);
            self.errorsCount = ko.observable(0);
            self.errorsStats = ko.observableArray([]);
            self.errorsStatsCount = ko.observable(0);
            self.errorsStatsDate = ko.observable(null);
            self.errorsStatsFormattedDate = ko.observable(null);
            self.deletor = ko.observable(null);

            //functions

            self.addError = function (envelope, append) {
                var model = new ErrorViewModel(envelope, self.sourceId, infoUrl);
                if (append)
                    self.errors.push(model);
                else
                    self.errors.unshift(model);
            };

            self.feedTitle = function () {
                return elmahr.isApplicationBlocked(sourceId)
                    ? elmahr.config.messageFeedPaused
                    : elmahr.config.messageFeedRunning;
            };

            self.toggleApplication = function (target, e) {
                var t = $(e.currentTarget);
                var c = t.attr('class');
                var n = c === play ? pause : play;

                t.switchClass(c, n, 0);

                elmahr.toggleApplication(target.sourceId, n);

                self.boxClass(self.boxClassBase + ' ' + (c === play ? 'grayed' : ''));

                t.attr('title', self.feedTitle());
            };

            self.toggleApplicationErrors = function (target, e) {
                var t = $(e.currentTarget);
                var c = t.attr('class');
                var n = c === show ? hide : show;

                t.switchClass(c, n, 0);

                $('#' + elmahr.config.appBoxIdPrefix + self.index).toggle('fast');
            };

            self.raiseTestException = function (target) {
                elmahr.raiseTestException(target.sourceId);
            };

            self.getErrorsResume = function (target) {
                elmahr.getErrorsResume(target.sourceId);
            };

            var clearerFunc = function (when, predicate) {
                var tbr = [],
                    errs = self.errors();

                for (var i = 0; i < errs.length; i++)
                    if (predicate(errs[i].rawTime, when))
                        tbr.push(errs[i]);

                for (var j = 0; j < tbr.length; j++)
                    self.errors.remove(tbr[j]);

                self.errorsCount(self.errorsCount() - tbr.length >= 0
                    ? self.errorsCount() - tbr.length
                    : 0);

                elmahr.clearErrors(when, predicate, self.sourceId);
            };

            self.askToClearAllErrors = function (target, event) {
                if (self.deletor())
                    return;
                elmahr.toggleBinLoading($(event.target), true);
                self.deletor(
                    {
                        'action':
                            function () {
                                elmahr.clearAllErrors(self.sourceId, function () {
                                    elmahr.toggleBinLoading($(event.target), false);
                                });
                                self.deletor(null);
                            }
                    });
            };

            self.askToClearErrorsBefore = function (when, onFail) {
                self.deletor({
                    'when': when,
                    'action': function () {
                        elmahr.clearErrorsBefore(self.sourceId, when, onFail);
                        self.deletor(null);
                    }
                });
            };

            self.askToClearErrorsAfter = function (when, onFail) {
                self.deletor({
                    'when': when,
                    'action': function () {
                        elmahr.clearErrorsAfter(self.sourceId, when, onFail);
                        self.deletor(null);
                    }
                });
            };

            self.clearAllErrors = function () {
                clearerFunc(null, function () { return true; });
            };

            self.clearErrorsBefore = function (when) {
                clearerFunc(when, function (l, r) { return l < r; });
            };

            self.clearErrorsAfter = function (when) {
                clearerFunc(when, function (l, r) { return l >= r; });
            };

            self.cancelDeletor = function () {
                self.deletor(null);
                elmahr.toggleBinLoading($("a.loading"), false);
            };

            //computed observables

            self.applicationClass = ko.computed(function () {
                return elmahr.isApplicationBlocked(sourceId) ? pause : play;
            }, this);

            self.titlePlayPause = ko.computed(self.feedTitle, this);

            self.updateErrorsStats = function (resume) {

                self.errorsStats([]);
                self.errorsStatsCount(0);

                if (resume) {
                    var barriers = resume.Barriers;

                    self.errorsStatsCount(resume.Count);
                    self.errorsStatsDate(resume.When);
                    self.errorsStatsFormattedDate(resume.When.trimMillisecondsFromIsoDate().format('%d-%n-%y'));

                    for (var i = 0; i < barriers.length; i++)
                        self.errorsStats.push(new ErrorBarrierViewModel(self,
                            barriers[i].When,
                            barriers[i].Offset,
                            barriers[i].CountBefore,
                            barriers[i].CountAfter));

                }
                selectedApp(self);
            };
        }

        //#endregion


        //#region Local vars

        var selected = (data && data.selected) || ko.observable(null),
            selectedApp = (data && data.selectedApp) || ko.observable(null),
            latest = (data && data.latest) || ko.observable(null),
            popupDisabled = false,
            cookieName = 'RememberMe',
            blockedErrorTypes = [],
            isErrorTypeBlocked = function (type) {
                return $.inArray(type, blockedErrorTypes) > -1;
            },
            blockedApplications = [],
            isApplicationBlocked = function (sourceId) {
                return $.inArray(sourceId, blockedApplications) > -1;
            },
            lowestIndex = -1,
            updateLowestIndex = function (e, append) {
                lowestIndex = append
                    ? e.id
                    : lowestIndex === -1
                        ? e.id
                        : lowestIndex;
            },
            clearer = function (sourceId, when, remoteClearer, localClearer) {
                var apps = elmahr.data.advertisedApplications();
                remoteClearer().done(function () {
                    for (var a = 0; a < apps.length; a++) {
                        var app = apps[a];
                        if (apps[a].sourceId === sourceId) {
                            localClearer(app);
                        }
                    }
                    elmahr.getErrorsResume(sourceId);
                    elmahr.refreshStats();
                });
            },
            askToClear = function (sourceId, action) {
                var apps = elmahr.data.advertisedApplications();
                for (var a = 0; a < apps.length; a++) {
                    var app = apps[a];
                    if (app.sourceId === sourceId)
                        action(app);
                }
            },
            logIt = function (message, severity) {
                $('#loading').text(message);

                var now = new Date();
                var utc = new Date(now.getTime() + now.getTimezoneOffset() * 60000);

                elmahr.onLog({
                    Message: message,
                    Date: utc.format("%d-%n-%y %H:%M:%S"),
                    Severity: severity
                });
            },
            disableLinks = false,
            mediaQuerySurrogate = function () {
                //media query, pair size with css
                var check = document.documentElement.clientWidth < 768;
                if (check !== disableLinks) {
                    disableLinks = check;
                    elmahr.disablePopup(disableLinks);
                }
            };

        //#endregion


        //#region Extending connector

        $.extend(elmahrConnector, {
            sortApplications: function (sortedApps) {
                return this.invoke('sortApplications', sortedApps);
            },
            toggleApplicationStatus: function (sourceId, active) {
                return this.invoke('toggleApplicationStatus', sourceId, active);
            },
            askForMoreErrors: function (lowest) {
                return this.invoke('askForMoreErrors', lowest);
            },
            askFullError: function (id, sourceId) {
                return this.invoke('askFullError', id, sourceId);
            },
            raiseTestException: function (sourceId) {
                return this.invoke('raiseTestException', sourceId);
            },
            getErrorsResume: function (sourceId) {
                return this.invoke('getErrorsResume', sourceId);
            },
            clearAllErrors: function (sourceId) {
                return this.invoke('clearAllErrors', sourceId);
            },
            clearErrorsBefore: function (sourceId, when) {
                return this.invoke('clearErrorsBefore', sourceId, when);
            },
            clearErrorsAfter: function (sourceId, when) {
                return this.invoke('clearErrorsAfter', sourceId, when);
            }
        });

        //#endregion

        elmahr.registerDataProvider({
            dashboard: function () {
                var notHistorical =
                    _.chain(elmahr.data.allErrors())
                        .filter(function (err) {
                            return !elmahr.data.latestHistoricalDateTime || err.isoTime > elmahr.data.latestHistoricalDateTime;
                        })
                        .value();
                return {
                    errors: function () {
                        return notHistorical;
                    },
                    historyFullErrorsStats: function () {
                        return elmahr.data.historyFullErrorsStats;
                    }
                };
            }
        });

        elmahr.registerComputationsReceiver({
            dashboard: function (x) {
                elmahr.data.types(x.types);

                elmahr.data.stats.removeAll();
                _.chain(x.stats)
                    .map(function (v) { return new ErrorTypeModel(v.key, v.value); })
                    .filter(function (kvp) { return kvp.key; })
                    .each(function (kvp) { elmahr.data.stats.push(kvp); });
            }
        });

        //#region elmahr object

        return {
            data: data || {
                advertisedApplications: ko.observableArray([]),
                allErrors: ko.observableArray([]),
                historyFullErrorsStats: [],
                latestHistoricalDateTime: null,
                unseenErrors: 'no',
                selected: selected,
                selectedApp: selectedApp,
                latest: latest,
                ready: false,
                total: ko.observable(0),
                stats: ko.observableArray([]),
                types: ko.observableArray([])
            },

            // methods

            disablePopup: function (disable) {
                popupDisabled = disable;
            },

            refreshStats: function () {
                var total = _.chain(this.data.allErrors())
                    .reduce(function (acc) { return acc + 1; }, 0)
                    .value();
                elmahr.data.total(total);

                elmahr.statsRefresher();
            },

            refreshStatsFor: function (computation) {
                elmahr.statsRefresher(computation);
            },

            sortApplications: function (sortedApps) {
                elmahrConnector.sortApplications(sortedApps);
            },

            start: function () {
                this.statusReady();
            },

            clearErrors: function (when, predicate, sourceId) {
                var tbra = [],
                    allErrs = this.data.allErrors();

                for (var i = 0; i < allErrs.length; i++)
                    if (predicate(allErrs[i].rawTime, when) && allErrs[i].sourceId === sourceId)
                        tbra.push(allErrs[i]);

                for (var j = 0; j < tbra.length; j++)
                    this.data.allErrors.remove(tbra[j]);

                this.data.total(elmahr.data.total() - tbra.length >= 0
                    ? elmahr.data.total() - tbra.length
                    : 0);
            },

            advertiseApplication: function (applicationName, infoUrl, sourceId, hasTestExceptionUrl, properties, active) {
                var apps = this.data.advertisedApplications(),
                    current = _.chain(apps)
                        .find(function (app) {
                            return app.sourceId === sourceId;
                        })
                        .value();

                if (!current) {

                    if (!active && !isApplicationBlocked(sourceId))
                        blockedApplications.push(sourceId);
                    else if (active && isApplicationBlocked(sourceId))
                        blockedApplications.splice($.inArray(sourceId, blockedApplications), 1);

                    elmahrConnector.toggleApplicationStatus(sourceId, active);

                    var applicationViewModel = new ApplicationViewModel(
                        applicationName,
                        infoUrl,
                        sourceId,
                        hasTestExceptionUrl,
                        properties["boxClass"],
                        this.data.advertisedApplications().length);

                    if (!$.isEmptyObject(this.plugins)) {
                        for (var i in this.plugins) {
                            var plugin = this.plugins[i];
                            plugin(elmahrConnector, applicationViewModel, properties);
                        }
                    }

                    this.data.advertisedApplications.push(applicationViewModel);

                }
            },

            addError: function (envelope, append) {
                var e = new ErrorViewModel(envelope, envelope.SourceId, envelope.InfoUrl);

                var apps = this.data.advertisedApplications();
                for (var a = 0; a < apps.length; a++) {
                    if (apps[a].sourceId === envelope.SourceId) {
                        apps[a].addError(envelope, append);
                    }
                }

                updateLowestIndex(e, append);

                if (append)
                    this.data.allErrors.push(e);
                else
                    this.data.allErrors.unshift(e);

                var sourceId = envelope.SourceId,
                    found = _.chain(apps)
                        .find(function (app) {
                            return app.sourceId === sourceId;
                        })
                        .value();

                if (found) {
                    if (!append) {
                        found.lastError(e);
                        latest(e);
                    }
                    found.errorsCount(found.errorsCount() + 1);

                    if (this.data.unseenErrors !== 'no') {
                        this.data.unseenErrors++;
                        document.title = '(' + this.data.unseenErrors + ') ' + this.config.title;
                    }
                }
            },

            askForMoreErrors: function () {
                elmahr.startTransfer();
                elmahrConnector.askForMoreErrors(lowestIndex);
                return false;
            },

            selectTarget: function (id, sourceId) {
                if (!popupDisabled)
                    elmahrConnector.askFullError(id, sourceId);
            },

            isErrorTypeBlocked: isErrorTypeBlocked,

            isApplicationBlocked: isApplicationBlocked,

            toggleErrorType: function (type, n) {
                if (n === 'pause' && !isErrorTypeBlocked(type))
                    blockedErrorTypes.push(type);
                else if (n === 'play' && isErrorTypeBlocked(type))
                    blockedErrorTypes.splice($.inArray(type, blockedErrorTypes), 1);
            },

            toggleApplication: function (sourceId, status) {
                if (status === pause && !isApplicationBlocked(sourceId)) {
                    blockedApplications.push(sourceId);
                    elmahrConnector.toggleApplicationStatus(sourceId, false);
                } else if (status === play && isApplicationBlocked(sourceId)) {
                    blockedApplications.splice($.inArray(sourceId, blockedApplications), 1);
                    elmahrConnector.toggleApplicationStatus(sourceId, true);
                }
            },

            raiseTestException: function (sourceId) {
                elmahrConnector.raiseTestException(sourceId);
            },

            getErrorsResume: function (sourceId) {
                if (!popupDisabled) {
                    elmahr.onGetErrorsResume(sourceId);
                    elmahrConnector.getErrorsResume(sourceId);
                }
            },

            notifyErrorsResume: function (sourceId, resume) {
                var apps = this.data.advertisedApplications();
                for (var a = 0; a < apps.length; a++) {
                    if (apps[a].sourceId === sourceId) {
                        apps[a].updateErrorsStats(resume);
                    }
                }
            },

            setBrowserVisible: function (visible) {
                this.data.unseenErrors = visible ? 'no' : 0;
                if (visible)
                    document.title = this.config.title;
            },

            askToClearAllErrors: function (sourceId, onFail) {
                askToClear(sourceId, function (app) { app.askToClearAllErrors(onFail); });
            },

            askToClearErrorsBefore: function (sourceId, when, onFail) {
                askToClear(sourceId, function (app) { app.askToClearErrorsBefore(when, onFail); });
            },

            askToClearErrorsAfter: function (sourceId, when, onFail) {
                askToClear(sourceId, function (app) { app.askToClearErrorsAfter(when, onFail); });
            },

            clearAllErrors: function (sourceId, onFail) {
                if (elmahr.config.enableDelete)
                    clearer(sourceId, null, function () { return elmahrConnector.clearAllErrors(sourceId).fail(onFail); }, function (app) { return app.clearAllErrors(); });
                else
                    onFail();
            },

            clearErrorsBefore: function (sourceId, when, onFail) {
                if (elmahr.config.enableDelete)
                    clearer(sourceId, when, function () { return elmahrConnector.clearErrorsBefore(sourceId, when).fail(onFail); }, function (app) { return app.clearErrorsBefore(when); });
                else
                    onFail();
            },

            clearErrorsAfter: function (sourceId, when, onFail) {
                if (elmahr.config.enableDelete)
                    clearer(sourceId, when, function () { return elmahrConnector.clearErrorsAfter(sourceId, when).fail(onFail); }, function (app) { return app.clearErrorsAfter(when); });
                else
                    onFail();
            },

            startup: function () {

                //knockoutjs bindings

                ko.bindingHandlers.applyTimeAgo = {
                    update: function (element) {
                        $(element).timeago($(element).hasClass('timeagoShort'));
                    }
                };

                ko.bindingHandlers.updateTotal = {
                    update: function (elem) {
                        if (elmahr.data.ready && !$(elem).hasClass("pulsating")) {
                            $(elem).addClass('pulsating');
                            $(elem).effect('pulsate', { times: 3 }, 400, function () {
                                $(elem).removeClass('pulsating');
                            });
                        }
                    }
                };

                // apply bindings

                ko.applyBindings(this.data);

            },

            callbacks: {
                advertiseApplications: function (apps) {
                    for (var a = 0; a < apps.length; a++) {
                        var app = apps[a];
                        elmahr.advertiseApplication(app.ApplicationName, app.InfoUrl, app.SourceId, app.HasTestExceptionUrl, app.Properties, app.Active);
                    }
                },

                notifyErrors: {
                    replacer: function (envelopes, append, startupErrors) {
                        elmahr.statusReceiving();
                        for (var k = 0; k < envelopes.length; k++) {
                            var envelope = envelopes[k],
                                s = '%2: Error from application %0 (%1):\r\n%3'.format(
                                    envelope.ApplicationName,
                                    envelope.SourceId,
                                    envelope.Error.Time.trimMillisecondsFromIsoDate().format("%d-%n-%y %H:%M:%S"),
                                    envelope.Error.Message);

                            elmahr.onLog(s);

                            if (!envelope.ApplicationName)
                                continue; //IE7 fix

                            if (isApplicationBlocked(envelope.SourceId))
                                continue;

                            var type = envelope.Error.ShortType;
                            if (isErrorTypeBlocked(type))
                                continue;

                            elmahr.addError(envelope, append);

                            if (startupErrors && (!elmahr.data.latestHistoricalDateTime || elmahr.data.latestHistoricalDateTime < envelope.Error.IsoTime)) {
                                elmahr.data.latestHistoricalDateTime = envelope.Error.IsoTime;
                            }
                        }

                        if (envelopes) {
                            if (envelopes.length) {
                                elmahr.statusRefreshingStats();
                                elmahr.refreshStats();
                            } else
                                elmahr.hideAskForMore();
                        }

                        if (elmahr.data.selectedApp()) {
                            _.chain(envelopes)
                                .map(function (e) { return e.SourceId; })
                                .uniq()
                                .each(function (id) { elmahr.getErrorsResume(id); });
                        }
                        elmahr.stopTransfer();
                    }
                },

                notifyFullError: function (error) {
                    if (!error)
                        return;

                    var errorModel = new ErrorViewModel(error);

                    selected(errorModel);
                    elmahr.onSelectError(errorModel);
                },

                notifyRememberMe: function (remember) {
                    elmahr.onRememberMe(remember);
                },

                notifyErrorsResume: function (sourceId, resume) {
                    elmahr.notifyErrorsResume(sourceId, resume);
                    elmahr.onGetErrorsResumeDone();
                },

                notifyFullErrorsStats: function (errorsStats) {
                    elmahr.data.historyFullErrorsStats = errorsStats;
                    elmahr.refreshStats();
                },

                log: function (message) {
                }
            },

            printMessage: {
                replacer: function (text, severity) {
                    elmahr.onLog(text, severity);
                    if (severity) {
                        $('#messages').text(text);
                        $('#messages').show();
                        window.setTimeout(function () {
                            $('#messages').hide();
                        },
                            5000);
                    }
                }
            },

            clearLog: function () {
                $('#log').text('');
            },

            onLog: function (message) {
                if (!elmahr.config.log)
                    return;
                var css = message.Severity
                        ? message.Severity
                        : '',
                    msg = message.Date
                        ? '[' + message.Date + '] ' + '<span class=\'' + css + '\'>' + message.Message + '</span>'
                        : message,
                    $log = $('#log');
                if ($log && $log.length) {
                    $log.append(msg + '<br/>');
                    var height = $log[0].scrollHeight;
                    $log.scrollTop(height);
                }
            },

            statusConnecting: function () {
                elmahrConnector.state.rememberMe = $.cookie(cookieName) || null;
                logIt('Connecting...');
            },
            statusConnected: function () {
                logIt('Connected, asking for errors...');
            },
            statusSendingCommand: function () {
                logIt('Sending command...');
            },
            statusStartingHubs: function () {
                logIt('Starting hubs...');
            },
            statusRefreshingStats: function () {
                logIt('Refreshing stats...');
            },
            statusReceiving: function () {
                logIt('Receiving errors...');
            },

            onSelectError: function () {
                $("#details abbr.timeago").timeago();
                $("#details").dialog("open");
            },

            onNewError: function (elem) {
                if (elem.nodeType === 1) {
                    $("abbr.timeago", elem).timeago();
                    $("abbr.timeagoShort", elem).timeago(true);
                    if (!$(elem).hasClass("onReconnect") && !$(elem).hasClass("pulsating")) {
                        $(elem).addClass('pulsating');
                        $(elem).effect('pulsate', { times: 3 }, 400, function () {
                            $(elem).removeClass('pulsating');
                        });
                    }
                }
            },

            onGetErrorsResume: function () {
                $("#errorsStats").dialog("open");
            },

            onGetErrorsResumeDone: function () {
                $("#loadingStats").hide();
                $("#statsLoaded").show();
            },

            onRememberMe: function (remember) {
                $('#rememberMe').attr('checked', remember);
            },

            hideAskForMore: function () {
                $('#askForMoreErrors').hide();
                $('#noMoreErrors').show();
            },

            startTransfer: function () {
                $('#loading').show();
                elmahr.data.ready = false;
            },

            stopTransfer: function () {
                $('#loading').hide();
                elmahr.data.ready = true;
            },

            getSortedApplications: function () {
                return $('#applicationBoxes')
                    .sortable('serialize', {
                        'attribute': 'data-sourceId'
                    })
                    .replace(/app\[\]\=/g, '')
                    .split('&');
            },

            toggleBinLoading: function (target, loading) {
                var remove = loading ? 'bin' : 'loading',
                    add = loading ? 'loading' : 'bin';
                target.removeClass(remove).addClass(add);
            },

            statusReady: function () {

                mediaQuerySurrogate();
                $(window).bind('resize', mediaQuerySurrogate);

                if (elmahr.config.log)
                    elmahr.toggleLog();

                $("#rememberMe").attr('checked', $.cookie(cookieName));

                //error details

                $("#details").dialog({
                    autoOpen: false,
                    height: 550,
                    width: 670,
                    modal: true
                });

                //application boxes

                $("#applicationBoxes")
                    .sortable({
                        revert: true,
                        opacity: 0.6,
                        update: function () {
                            elmahr.sortApplications(elmahr.getSortedApplications());
                        }
                    })
                    .disableSelection();

                //errors stats

                $("#errorsStats")
                    .dialog({
                        autoOpen: false,
                        height: 550,
                        width: 670,
                        modal: true,
                        close: function () {
                            elmahr.data.selectedApp(null);
                        }
                    });

                //hooking events

                $("#toggleDescriptiveText").click(function () {
                    $('.intro').toggle();
                    return false;
                });

                var self = this,
                    apps = self.data.advertisedApplications(),
                    ec = elmahrConnector,
                    isBlocked = isApplicationBlocked;
                $("#rememberMe").change(function () {
                    if (!$.cookie(cookieName)) {
                        var key = String.prototype.newGuid();
                        ec.state.rememberMe = key;
                        //set
                        $.cookie(cookieName, key, { expires: 7 });
                        for (var a = 0; a < apps.length; a++) {
                            var app = apps[a];
                            ec.toggleApplicationStatus(app.sourceId, !isBlocked(app.sourceId));
                        }
                        ec.sortApplications(self.getSortedApplications());
                    } else {
                        //reset
                        $.removeCookie(cookieName);
                        ec.state.rememberMe = null;
                    }
                });

                $("#openRememberMeInfo").click(function () {
                    $('#rememberMeInfo').toggle();
                    return false;
                });

                $("#logBin").click(function () {
                    $('#log').text('');
                    return false;
                });


                // binding
                elmahr.startup();

                $('#wait').hide();
                $('#logPad').show();
                $('#dashboard').show();

            }

        };

        //#endregion

    });

    //#endregion

}());