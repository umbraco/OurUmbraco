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

    var compliments = [
        "Awesome",
        "Great job",
        "Well done",
        "Kudos",
        "Nice",
        "Woohooo",
        "You Rock",
        "Lovely",
        "Genius",
    "Thank you"]

    var useMockApi = true;

    // HighFives - JS functionality for the high fives module.
    var HighFives = {
        list: [],
        selectedMember: false,
        suggestions: [],
        // init - Starts the high fives app functionality.
        init: function() {
            $(document).ready(function () {
                if (HighFives.doesHaveHighFive()) {
                    HighFives.bindOnMentionChange();
                    HighFives.bindOnMemberSelect();
                    HighFives.printPhrases(HighFives.shuffle(placeholderNames), $('#high-five-mention'));
                    HighFives.getCategories(function(response) {
                        HighFives.buildCategoryDropdown(response);
                        HighFives.getRecentHighFiveActivity(0, function(response) {
                            HighFives.list = response.highFives;
                            HighFives.buildActivityList(HighFives.list);
                            HighFives.checkForNewHighFivesPeriodically(30);
                        });
                    });
                }
            });
        },

        addToPlaceholder: function (toAdd, el) {
            el.attr('placeholder', el.attr('placeholder') + toAdd);
            // Delay between symbols "typing"
            return new Promise(resolve => setTimeout(resolve, 100));
        },

        bindOnMentionChange: function () {
            jQuery('#high-five-mention').keyup(function(e) {
                HighFives.selectedMember = false;
                HighFives.getMember(e.target.value);
                HighFives.selectMemberIfMatches(e.target.value);
            });
        },

        bindOnMemberSelect: function() {
            jQuery('#high-five-form .suggestions-list button').unbind('click');
            jQuery('#high-five-form .suggestions-list button').click(function(e) {
                var id = e.target.getAttribute('data-id');
                HighFives.selectedMember = {
                    id: e.target.getAttribute('data-id'),
                    name: e.target.innerHTML
                };
                HighFives.selectMember(HighFives.selectedMember);
            });
            // unbind from previous versions just in case
        },

        // buildActivityList - Builds a list of list items that represent the activity list and adds them to an activity list for users to view.
        buildActivityList: function () {
            var highFives = HighFives.list;
            if (highFives && highFives.length > 0) {
                var template = _.template($("script.high-five-template").html());
                var list = document.querySelector("#high-five-activity .high-five-activity-list");
                list.innerHTML = '';
                for (var i = 0; i < highFives.length; i++) {
                    var highFive = highFives[i];

                    var highFiveObject = {
                        avatar: "/media/avatar/144494.png",
                        compliment: compliments[Math.floor(Math.random() * compliments.length)],
                        name: placeholderNames[Math.floor(Math.random() * placeholderNames.length)],
                        highFiver: placeholderNames[Math.floor(Math.random() * placeholderNames.length)],
                        url: highFive.url,
                        type: highFive.type,
                        timestamp: "2 minutes ago"
                    };

                    list.innerHTML += template(highFiveObject);
                }
            }
        },

        buildSuggestionsList: function () {
            var suggestions = HighFives.suggestions;
            var list = document.querySelector("#high-five-form .suggestions-list");
            list.innerHTML = '';
            for (var i = 0; i < suggestions.length; i++) {
                var suggestion = suggestions[i];
                list.innerHTML += '<li>' + 
                '<button type="button" data-id="' +  suggestion.MemberId + '">' + suggestion.Username + 
                '</button></li>';
            }
            HighFives.bindOnMemberSelect();
        },

        // buildCategoryDropdown - Builds a list of options for the category dropdown.
        buildCategoryDropdown: function(categories) {
            if (categories && categories.length > 0) {
                var dropdown = jQuery('#high-five-task');
                var options = '<option value="" disabled selected>...</option>';
                for (var i = 0; i < categories.length; i++) {
                    var category = categories[i];
                    options += '<option value="' + category.Id + '">' + category.CategoryText + '</option>';
                }
                dropdown.html(options);
            }
        },

        // checkForNewHighFivesPeriodically - Polls the API endpoint for new high fives periodically
        checkForNewHighFivesPeriodically: function (seconds) {
            window.setTimeout(function() {
                HighFives.getRecentHighFiveActivity(0, function(response) {
                    HighFives.list = _.unionBy(HighFives.list, response.highFives, 'id').slice(0, 10);
                    HighFives.buildActivityList(HighFives.list);
                    HighFives.checkForNewHighFivesPeriodically(30);
                });
            }, (seconds * 1000));
        },

        clearPlaceholder: function (el) {
            el.attr("placeholder", "");
        },

        // getCategories - Get the categories for the high fives.
        getCategories: function(onSuccess) {
            if (useMockApi) {
                onSuccess(ApiMock.getCategories());
            } else {
                jQuery.get('/umbraco/api/HighFiveFeedAPI/GetCategories', onSuccess);
            }
        },

        getMember: function(member) {
            if (member.length > 2) {
                if (useMockApi) {
                    HighFives.suggestions = ApiMock.getUmbracians();
                    HighFives.buildSuggestionsList();
                } else {
                    jquery.get('/Umbraco/Api/highFiveFeedApi/GetUmbracians?name=' + member, function (umbracians) {
                        HighFives.suggestions = umbracians;
                        HighFives.buildSuggestionsList();
                    });
                }
            }
        },

        // getRecentHighFiveActivity - Gets the most recent high fives via API.
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

        // selectMember
        selectMember: function (member) {
            var list = document.querySelector("#high-five-form .suggestions-list");
            list.innerHTML = '';
            jQuery('#high-five-mention').val(member.name);
        },

        // selectMemberIfMatches - If the `value` matches the name of a user in the suggestions list, select them.
        selectMemberIfMatches: function(value) {
            var suggestions = HighFives.suggestions;
            if (suggestions && suggestions.length > 0) {
                for (var i = 0; i < suggestions.length; i++) {
                    var suggestion = suggestions[i];
                    if (suggestion.Username.toLowerCase() == value.toLowerCase()) {
                        HighFives.selectedMember = {
                            id: suggestion.MemberId,
                            name: suggestion.Username
                        };
                        HighFives.selectMember(HighFives.selectedMember);
                    }
                }
            }
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

    var memberSearch = _.debounce(HighFives.getMember, 300);

    var ApiMock = {
        getCategories: function() {
            return [
                { "Id": 1, "CategoryText": "A Package" }, 
                { "Id": 2, "CategoryText": "A Talk" }, 
                { "Id": 3, "CategoryText": "A Blog Post" }, 
                { "Id": 4, "CategoryText": "A Meetup" }, 
                { "Id": 5, "CategoryText": "A Skrift Article" }, 
                { "Id": 6, "CategoryText": "A Tutorial" }, 
                { "Id": 7, "CategoryText": "Advice" }, 
                { "Id": 8, "CategoryText": "A Video" }, 
                { "Id": 9, "CategoryText": "A PR" } 
            ];
        },
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
        },
        getUmbracians: function() {
            return [
                {
                    MemberId: '123',
                    Username: 'Fred Johnson',
                },
                {
                    MemberId: '124',
                    Username: 'Fred Samson',
                },
                {
                    MemberId: '125',
                    Username: 'Fredina Hartvig'
                }
            ];
        }
    };

    // Init
    HighFives.init();

})();