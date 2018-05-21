(function() {

    var placeholderNames = [
        "Marcin Zajkowski",
        "Callum Whyte",
        "Anders Bjerner",
        "Emma Garland",
        "Lars - Erik Aabech",
        "Matt Brailsford",
        "Carole Rennie Logan",
        "Emma Burstow",
        "Lee Kelleher",
        "Jeffrey Schoemaker",
        "Erica Quessenberry",
        "Kyle Weems",
        "Mike Masey",
        "Niels Hartvig",
        "Jeavon Leopold",
        "Damiaan Peeters",
        "Marc Goodson",
        "Mads Rasmussen",
        "Shannon Deminick",
        "Stéphane Gay",
        "Sebastiaan Janssen",
        "Jacob Midtgaard - Olesen",
        "Ilham Boulghallat"
    ];

    var useMockApi = true;

    /**
     * @var HighFives - JS functionality for the high fives module.
     */
    var HighFives = {
        /**
         * @method init
         * @returns {void}
         */
        init: function() {
            $(document).ready(function () {
                if (HighFives.doesHaveHighFive()) {
                    HighFives.printPhrases(HighFives.shuffle(placeholderNames), $('#high-five-mention'));
                    HighFives.getRecentHighFiveActivity(0, function(response) {
                        console.info('response', response);
                        HighFives.buildActivityList(response.highFives);
                    })
                }
            });
        },

        addToPlaceholder: function (toAdd, el) {
            el.attr('placeholder', el.attr('placeholder') + toAdd);
            // Delay between symbols "typing"
            return new Promise(resolve => setTimeout(resolve, 100));
        },

        buildActivityList: function (highFives) {
            if (highFives && highFives.length > 0) {
                var list = document.querySelector("#high-five-activity .high-five-activity-list");
                list.innerHTML = '';
                for (var i = 0; i < highFives.length; i++) {
                    var highFive = highFives[i];
                    list.innerHTML += '<li><span class="from">' + highFive.from + '</span> has highfived ' + 
                    '<img class="avatar" src="' + highFive.toAvatarUrl + '" /><span class="to">' + highFive.to +  '</span>' + 
                    ' for a <span class="type">' + highFive.type + '</span>' + 
                    ((highFive.url && highFive.url !== '') ? ' at <a href="' + highFive.url + '" target="_blank">' + highFive.url + '</a>' : '') + 
                    '.</li>';
                }
            }
        },

        clearPlaceholder: function (el) {
            el.attr("placeholder", "");
        },

        /**
         * @method getRecentHighFiveActivity - Gets the most recent high fives
         * from the API.
         * @param {number=} page
         * @param {function=} onSuccess
         * @param {function=} onError
         * @returns {void}
         */
        getRecentHighFiveActivity: function(page, onSuccess, onError) {
            if (typeof page === 'undefined') {
                page = 0;
            }
            if (useMockApi) {
                onSuccess(ApiMock.getHighFiveFeed());
            } else {
                orcAjax.get('/umbraco/api/HighFiveFeedAPI/GetHighFiveFeed?page=' + page, onSuccess(response), function(error) {
                    console.error(error);
                });
            }
        },

        /**
         * @method doesHaveHighFive
         * @returns {boolean}
         */
        doesHaveHighFive: function() {
            var highFive = document.querySelector('section[data-high-five]');
            if (highFive && typeof highFive !== 'null' && typeof highFive !== 'undefined') {
                return true;
            }
            return false;
        },
        
        printPhrase: function (phrase, el) {
            return new Promise(resolve => {
                // Clear placeholder before typing next phrase
                HighFives.clearPlaceholder(el);
                let letters = phrase.split('');
                // For each letter in phrase
                letters.reduce(
                    (promise, letter, index) => promise.then(_ => {
                        // Resolve promise when all letters are typed
                        if (index === letters.length - 1) {
                            // Delay before start next phrase "typing"
                            setTimeout(resolve, 1000);
                        }
                        return HighFives.addToPlaceholder(letter, el);
                    }),
                    Promise.resolve()
                );
            });
        },
        
        printPhrases: function (phrases, el) {
            // For each phrase
            // wait for phrase to be typed
            // before start typing next
            phrases.reduce(
                (promise, phrase) => promise.then(_ => HighFives.printPhrase(phrase, el)),
                Promise.resolve()
            );
        },
  
        shuffle: function(array) {
            var currentIndex = array.length, temporaryValue, randomIndex;

            // While there remain elements to shuffle...
            while (0 !== currentIndex) {
    
                // Pick a remaining element...
                randomIndex = Math.floor(Math.random() * currentIndex);
                currentIndex -= 1;
    
                // And swap it with the current element.
                temporaryValue = array[currentIndex];
                array[currentIndex] = array[randomIndex];
                array[randomIndex] = temporaryValue;
            }
    
            return array;
        }
    };

    /**
     * @class orcAjax
     * @description A basic AJAX handler. Learn more at https://github.com/Offroadcode/ORC-AJAX
     * @author Offroadcode Limited (http://offroadcode.com)
     */
    var orcAjax = {
        request: false,
        /**
         * @method get
         * @param {string} url
         * @param {function=string} onSuccess
         * @param {function=string} onError
         * @param {boolean} shouldLogResponse
         * @returns {void}
         * @description Sends an AJAX GET request to the url provided. The response is 
         * provided to either the onSuccess or onError callback, depending on the status.
         */
        get: function(url, onSuccess, onError, shouldLogResponse) {
            orcAjax.request = new XMLHttpRequest();
            orcAjax.request.onreadystatechange = function (e) {
                if (e.target) {
                    var reply = e.target;
                    var DONE = 4; // readyState 4 means the request is done.
                    var OK = 200; // status 200 is a successful return.
                    if (reply.readyState === DONE) {
                        if (reply.status === OK) {
                                var response = false;
                                if (reply.responseType === 'json') {
                                    response = reply.response;
                                } else {
                                    response = JSON.parse(reply.responseText);
                                }
                                if (shouldLogResponse) {
                                    console.log('Success: orcAjax.get(' + url + ') response:', response);
                                }
                                if (onSuccess) {
                                    onSuccess(response);
                                }
                        } else {
                            if (shouldLogResponse) {
                                console.log('Error: orcAjax.get(' + url + '):', reply.status);
                            }
                            if (onError) {
                                onError(reply);
                            }
                        }
                    }

                }
            }
            orcAjax.request.open('GET', url, true);
            orcAjax.request.responseType = 'json';
            orcAjax.request.send();
        },
        /**
         * @method post
         * @param {string} url
         * @param {JSON} data - the data to submit in JSON format.
         * @param {function=string} onSuccess
         * @param {function=string} onError
         * @param {boolean} shouldLogResponse
         * @returns {void}
         * @description Sends an AJAX POST request to the url provided. The response is 
         * provided to either the onSuccess or onError callback, depending on the status.
         */    
        post: function(url, data, onSuccess, onError, shouldLogResponse) {
            orcAjax.request = new XMLHttpRequest();
            orcAjax.request.onreadystatechange = function (e) {
                if (e.target) {
                    var reply = e.target;
                    var DONE = 4; // readyState 4 means the request is done.
                    var OK = 200; // status 200 is a successful return.
                    if (reply.readyState === DONE) {
                        if (reply.status === OK) {
                                var response = false;
                                if (reply.responseType === 'json') {
                                    response = reply.response;
                                } else {
                                    response = JSON.parse(reply.responseText);
                                }
                                if (shouldLogResponse) {
                                    console.log('Success: orcAjax.get(' + url + ') response:', response);
                                }
                                if (onSuccess) {
                                    onSuccess(response);
                                }
                        } else {
                            if (shouldLogResponse) {
                                console.log('Error: orcAjax.get(' + url + '):', reply.status);
                            }
                            if (onError) {
                                onError(reply);
                            }
                        }
                    }

                }
            }
            orcAjax.request.open('POST', url, true);
            orcAjax.request.setRequestHeader("Content-type", "application/json");
            orcAjax.request.responseType = 'json';
            orcAjax.request.send(JSON.stringify(data));                
        }
    };

    var ApiMock = {
        getHighFiveFeed: function() {
            return {
                count: 100,
                pageCount: 10,
                currentPage: 0,
                highFives: [
                    {
                    id: '123',
                    from: 'Name of High Fiver',
                    to: 'Name of High Fivee',
                    fromAvatarUrl: '/avatar_of_high_fiver.jpg',
                    toAvatarUrl: '/avatar_of_high_fivee.jpg',
                    type: 'Blog post',
                    url: 'http://optional.url.for.high.five.com'
                    }
                ]
            };
        }
    };

    // Init
    HighFives.init();

})();