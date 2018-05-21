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

    // HighFives - JS functionality for the high fives module.
    var HighFives = {
        list: [],
        // init - Starts the high fives app functionality.
        init: function() {
            $(document).ready(function () {
                if (HighFives.doesHaveHighFive()) {
                    HighFives.printPhrases(HighFives.shuffle(placeholderNames), $('#high-five-mention'));
                    HighFives.getRecentHighFiveActivity(0, function(response) {
                        HighFives.list = response.highFives;
                        HighFives.buildActivityList(HighFives.list);
                        HighFives.checkForNewHighFivesPeriodically(30);
                    });
                }
            });
        },

        addToPlaceholder: function (toAdd, el) {
            el.attr('placeholder', el.attr('placeholder') + toAdd);
            // Delay between symbols "typing"
            return new Promise(resolve => setTimeout(resolve, 100));
        },

        // buildActivityList - Builds a list of list items that represent the activity list and adds them to an activity list for users to view.
        buildActivityList: function () {
            var highFives = HighFives.list;
            if (highFives && highFives.length > 0) {
                var list = jQuery("#high-five-activity .high-five-activity-list");
                list.empty();
                var items = [];
                for (var i = 0; i < highFives.length; i++) {
                    var highFive = highFives[i];
                    items += '<li><span class="from">' + highFive.from + '</span> has highfived ' + 
                    '<img class="avatar" src="' + highFive.toAvatarUrl + '" /><span class="to">' + highFive.to +  '</span>' + 
                    ' for a <span class="type">' + highFive.type + '</span>' + 
                    ((highFive.url && highFive.url !== '') ? ' at <a href="' + highFive.url + '" target="_blank">' + highFive.url + '</a>' : '') + 
                    '.</li>';
                }
                list.append(items);
            }
        },

        // checkForNewHighFivesPeriodically - Polls the API endpoint for new high fives periodically
        checkForNewHighFivesPeriodically: function (seconds) {
            window.setTimeout(function() {
                HighFives.getRecentHighFiveActivity(0, function(response) {
                    HighFives.list = _.union(HighFives.list, response.highFives).slice(0, 10);
                    HighFives.buildActivityList(HighFives.list);
                    HighFives.checkForNewHighFivesPeriodically(30);
                });
            }, (seconds * 1000));
        },

        clearPlaceholder: function (el) {
            el.attr("placeholder", "");
        },

        // @method getRecentHighFiveActivity - Gets the most recent high fives via API.
        getRecentHighFiveActivity: function(page, onSuccess) {
            page = typeof page === 'undefined' ? 0 : page;
            if (useMockApi) {
                onSuccess(ApiMock.getHighFiveFeed());
            } else {
                jQuery.get('/umbraco/api/HighFiveFeedAPI/GetHighFiveFeed?page=' + page, onSuccess);
            }
        },

        // doesHaveHighFive - returns true if highFive element exists
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