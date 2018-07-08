(function() {

    var useMockApi = false;

    // HighFives - JS functionality for the high fives module.
    var HighFives = {
        list: [],
        rawList: [],
        selectedMember: false,
        suggestions: [],
        prevLinkUrl: "",
        // init - Starts the high fives app functionality.
        init: function() {
            $(document).ready(function () {
                if (HighFives.doesHaveHighFive()) {
                    HighFives.bindOnMentionChange();
                    HighFives.bindOnMemberSelect();
                    HighFives.bindOnSubmitForm();
                    HighFives.bindOnLinkChange();
                    HighFives.defaultPreview();
                    HighFives.activityListNode = document.querySelector("#high-five-activity .high-five__activity-list");
                    HighFives.highFiveActivityNode = document.querySelector("#high-five-activity");
                    HighFives.getCurrentMember();
                    HighFives.getRandomUmbracians(function(people) {
                        HighFives.printPhrases(HighFives.shuffle(_.map(people, 'Username')), $('#high-five-mention'));
                        HighFives.getCategories(function(response) {
                            var categories = typeof response == 'string' ? JSON.parse(response) : response;

                            HighFives.buildCategoryDropdown(categories);
                            HighFives.getRecentHighFiveActivity(0, function(response) {
                                var activity = typeof response == 'string' ? JSON.parse(response): response;
                                HighFives.updateHighFiveList(activity.HighFives);
                                HighFives.checkForNewHighFivesPeriodically(30);
                            });
                        });
                    });
                }
            });
        },
        defaultPreview()
        {
            HighFives.preview = {};
            HighFives.preview.To = "";
            HighFives.preview.ToAvatarUrl = "";
            HighFives.preview.Category = "";
            HighFives.preview.LinkUrl = "";
            HighFives.preview.LinkTitle = "";
            HighFives.getCurrentMember();
            HighFives.previewNode = HighFives.createPreviewNode();
        },
        updateHighFiveList(newHighFives) {
            if(_.isEqual(HighFives.rawList, newHighFives) === false) {
                HighFives.rawList = _.clone(newHighFives, true);
                HighFives.list = HighFives.addComplimentsToList(newHighFives.slice(0, 6));
                HighFives.buildactivityListNode(HighFives.list);
            }
        },

        addComplimentsToList: function (list) {
            var complimentedList = [];
            for (var i = 0; i < list.length; i++) {
                list[i].To = HighFives.getRandomCompliment() + ' ' + list[i].To;
            }
            return list;
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
        bindOnLinkChange: function () {
            jQuery('#high-five-url').keyup(_.debounce(HighFives.getUrlTitle, 300));
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

        bindOnCategorySelect: function () {
            var categorySeleted = jQuery('#high-five-task option:selected').text();
            HighFives.preview.Category = categorySeleted;
            HighFives.updatePreviewNode();
        },

        bindOnSubmitForm: function() {
            jQuery('#high-five-form').submit(function(e) {
                e.preventDefault();
                if (HighFives.isFormValid()) {
                    HighFives.submitHighFive(HighFives.selectedMember.id, jQuery('#high-five-task').val(), jQuery('#high-five-url').val(), HighFives.linkTitle, function () {
                        HighFives.resetForm();
                        HighFives.removePreviewNode();
                        HighFives.getRecentHighFiveActivity(0, function(response) {
                            var activity = typeof response == 'string' ? JSON.parse(response): response;
                            HighFives.updateHighFiveList(activity.HighFives);
                            HighFives.buildactivityListNode(HighFives.list);
                        });   
                    });
                }
            });
        },

        // buildactivityListNode - Builds a list of list items that represent the activity list and adds them to an activity list for users to view.
        buildactivityListNode: function () {
            var highFives = HighFives.list;

            if (highFives && highFives.length > 0) {
                HighFives.activityListNode.innerHTML = '';
                for (var i = 0; i < highFives.length; i++) {
                    var highFive = highFives[i];
                    HighFives.activityListNode.innerHTML += HighFives.createHighFivePanelHtml(highFive);
                }

            }
        },
        createPreviewNode() {
            var preview = document.createElement("ul");
            preview.classList.add("high-five__activity-list");
            preview.classList.add("high-five__activity-list--preview");
            return preview;
        },
        updatePreviewNode: function() {
            HighFives.previewNode.innerHTML = '';
            var highFive = HighFives.preview;
                HighFives.previewNode.innerHTML += '<p>Preview of your High Five: </p>';
                HighFives.previewNode.innerHTML += HighFives.createHighFivePanelHtml(highFive);

            if(document.body.contains(HighFives.previewNode) === false) {
                HighFives.highFiveActivityNode.prepend(HighFives.previewNode);
            }
        },
        removePreviewNode: function () {
            HighFives.previewNode.remove();
            HighFives.defaultPreview();
        },
        createHighFivePanelHtml(highFive) {
            return '<li class="high-five-panel">' + 
            '<div class="high-five-panel__avatar"><img src="' + highFive.ToAvatarUrl + '?v=test&amp;width=100&amp;height=100&amp;mode=crop&amp;upscale=true" ' +
            'srcset="' + highFive.ToAvatarUrl + '?v=test&amp;width=200&amp;height=200&amp;mode=crop&amp;upscale=true 2x, ' + 
            highFive.ToAvatarUrl + '?v=test&amp;width=300&amp;height=300&amp;mode=crop&amp;upscale=true 3x" alt="' + highFive.To + '"></div>' + 
            '<div class="high-five-panel__meta"><div class="high-five-panel__text">' + 
            '<h3 class="high-five-panel__header">' + highFive.To + '</h3>' + 
            '<p>' + highFive.From + ' High Fived you for ' + highFive.Category +' : <a href="' + highFive.LinkUrl + '"><span class="audio-only">' + highFive.LinkTitle + '</span><span aria-hidden="true">' + _.trunc(highFive.LinkTitle, {length: 70}) + '</span></a>' + /*, 2 minutes ago*/ '</p>' + 
            '</div></div></li>';
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

        clearSuggestionsList: function () {
            jQuery('#high-five-form .suggestions-list button').unbind('click');
            var list = document.querySelector("#high-five-form .suggestions-list");
            list.innerHTML = '';
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
            $("#high-five-task").change(function () {
                HighFives.bindOnCategorySelect();
            });
        },

        // checkForNewHighFivesPeriodically - Polls the API endpoint for new high fives periodically
        checkForNewHighFivesPeriodically: function (seconds) {
            window.setTimeout(function() {
                HighFives.getRecentHighFiveActivity(0, function(response) {
                    var feed = typeof response == 'string' ? JSON.parse(response) : response;
                    // HighFives.list = HighFives.unionBy(HighFives.list, HighFives.addComplimentsToList(feed.HighFives)).slice(0, 6);
                    HighFives.updateHighFiveList(feed.HighFives);
                    HighFives.checkForNewHighFivesPeriodically(30);
                });
            }, (seconds * 1000));
        },

        clearPlaceholder: function (el) {
            el.attr("placeholder", "");
        },

        // doesHaveHighFive - returns true if highFive element exists
        doesHaveHighFive: function() {
            var highFive = document.querySelector('section[data-high-five]');
            if (highFive && typeof highFive !== 'null' && typeof highFive !== 'undefined') {
                return true;
            }
            return false;
        },        

        // getCategories - Get the categories for the high fives.
        getCategories: function(onSuccess) {
            if (useMockApi) {
                onSuccess(ApiMock.getCategories());
            } else {
                jQuery.getJSON('/umbraco/api/HighFiveFeedAPI/GetCategories', onSuccess);
            }
        },

        getMember: function(member) {
            if (member.length > 2) {
                if (useMockApi) {
                    HighFives.suggestions = ApiMock.getUmbracians();
                    HighFives.buildSuggestionsList();
                } else {
                    jQuery.get('/Umbraco/Api/highFiveFeedApi/GetUmbracians?name=' + member, function (umbracians) {
                        HighFives.suggestions = umbracians;
                        HighFives.buildSuggestionsList();
                    });
                }
            } else {
                HighFives.clearSuggestionsList();
            }
        },
        getUrlTitle: function (event) {
            var url = event.target.value || "";

            if (event.target.validity.valid === true && url.length > 10 && HighFives.prevLinkUrl !== url) {
                HighFives.prevLinkUrl = url;
                jQuery.get('/Umbraco/Api/highFiveFeedApi/GetTitleTag?url=' + encodeURIComponent(url), function (title) {
                    HighFives.linkTitle = title;
                    HighFives.preview.LinkTitle = title;
                    HighFives.preview.LinkUrl = url;
                    HighFives.updatePreviewNode();
                });
            }
        },
        getCurrentMember: function () {
                jQuery.get('/Umbraco/Api/highFiveFeedApi/GetCurrentMember', function (member) {
                    HighFives.currentMember = member;
                    HighFives.preview.From = HighFives.currentMember.Username;
                });

            },
        getRandomCompliment: function() {
            var compliments = [
                'Lovely',
                'Woohoo!',
                '#h5yr!',
                'Go',
                'Awesome!',
                'Rockstar',
                'Yass!',
                'Most excellent',
                'Excellent!',
                'Supertak!',
                'Cheers!',
                'Wow!',
                'Yahoo!',
                'Yee haw!',
                'Boo ya!',
                'Hoorah!',
                'Huzzah!',
                'Tada!',
                'Yippee!',
                'Squeee!',
                'Yowza!',
                'Damn Skippy!',
                'Hells yeah!',
                'Bingo!',
                'KAPOW!',
                'Bravo!',
                'That\'s magic!',
                'Wizard!',
                'Nailed it!',
                'Well done!',
                'Like a boss!',
                'Congrats!',
                'Go you!',
                'Get you!',
                'Yay!'
            ];
            var rnd = Math.floor(Math.random() * compliments.length);
            return compliments[rnd];  
        },

        // getRandomUmbracians - Get a random list of Umbracians to use for placeholder.
        getRandomUmbracians: function(onSuccess) {
            if (useMockApi) {
                onSuccess(ApiMock.getRandomUmbracians());
            } else {
                jQuery.get('/umbraco/api/HighFiveFeedAPI/getRandomUmbracians', onSuccess);
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

        isFormValid: function() {
            if (HighFives.selectedMember) {
                if (jQuery('#high-five-url').val() !== '') {
                    if (typeof jQuery('#high-five-task').val() !== 'object') {
                        return true;
                    }
                }
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

        resetForm: function () {
            HighFives.selectedMember = false;
            HighFives.suggestions = [];
            jQuery('#high-five-url').val('');
            jQuery('#high-five-mention').val('');
            HighFives.buildSuggestionsList();
        },

        // selectMember
        selectMember: function (member) {
            var list = document.querySelector("#high-five-form .suggestions-list");
            list.innerHTML = '';
            jQuery('#high-five-mention').val(member.name);
            HighFives.preview.To = member.name;
           
            jQuery.get('/Umbraco/Api/highFiveFeedApi/GetMemberAvatar?memberId=' + member.id, function (memberAvatar) {
                HighFives.preview.ToAvatarUrl = memberAvatar;
                HighFives.updatePreviewNode();
            });
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

        // submitHighFive
        submitHighFive: function(to, action, url, linkTitle, onSuccess) {
            if (useMockApi) {
                onSuccess(ApiMock.submitHighFive());
            } else {
                jQuery.post('/umbraco/api/HighFiveFeedAPI/SubmitHighFive?toUserId=' + to + '&action=' + action + '&url=' + url + '&linkTitle=' + linkTitle, onSuccess);   
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
        },

        unionBy: function(oldArray, newArray) {
            for (var n = 0; n < newArray.length; n++) {
                var itemToAdd = newArray[n];
                var isUnique = true;
                for (var o = 0; o < oldArray.length; o++) {
                    var oldItem = oldArray[o];
                    if (oldItem.Id === itemToAdd.Id) {
                        isUnique = false;
                    }
                }
                if (isUnique) {
                    oldArray.unshift(itemToAdd);
                }
            }
            return oldArray;
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
                Count: 100,
                PageCount: 10,
                CurrentPage: 0,
                HighFives: [
                    {
                    Id: '123',
                    From: 'Name of High Fiver',
                    To: 'Name of High Fivee',
                    FromAvatarUrl: '/avatar_of_high_fiver.jpg',
                    ToAvatarUrl: '/avatar_of_high_fivee.jpg',
                    Type: 'Blog post',
                    Url: 'http://optional.url.for.high.five.com'
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
        },
        getRandomUmbracians: function() {
            return [
                {MemberId: '0001', Username: "Marcin Zajkowski"},
                {MemberId: '0002', Username: "Callum Whyte"},
                {MemberId: '0003', Username: "Anders Bjerner"},
                {MemberId: '0004', Username: "Emma Garland"},
                {MemberId: '0005', Username: "Lars - Erik Aabech"},
                {MemberId: '0006', Username: "Matt Brailsford"},
                {MemberId: '0007', Username: "Carole Rennie Logan"},
                {MemberId: '0008', Username: "Emma Burstow"},
                {MemberId: '0009', Username: "Lee Kelleher"},
                {MemberId: '0010', Username: "Jeffrey Schoemaker"},
                {MemberId: '0011', Username: "Erica Quessenberry"},
                {MemberId: '0012', Username: "Kyle Weems"},
                {MemberId: '0013', Username: "Mike Masey"},
                {MemberId: '0014', Username: "Niels Hartvig"},
                {MemberId: '0015', Username: "Jeavon Leopold"},
                {MemberId: '0016', Username: "Damiaan Peeters"},
                {MemberId: '0017', Username: "Marc Goodson"},
                {MemberId: '0018', Username: "Mads Rasmussen"},
                {MemberId: '0019', Username: "Shannon Deminick"},
                {MemberId: '0020', Username: "Stéphane Gay"},
                {MemberId: '0021', Username: "Sebastiaan Janssen"},
                {MemberId: '0022', Username: "Jacob Midtgaard - Olesen"},
                {MemberId: '0023', Username: "Ilham Boulghallat"}
            ]
        },
        submitHighFive: function() {
            return {};
        }
    };

    // Init
    HighFives.init();

})();