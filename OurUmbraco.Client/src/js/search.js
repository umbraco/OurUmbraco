function redirectToSearch(ev) {
    if (ev.type === "click" || ev.keyCode === 13) {
        ev.preventDefault();

        var query = "";
        if (ev.type === "click") {
            query = $(".search-input").val();
        } else {
            query = this.value.replace(":", " ");
        }

        var categoryName = getParameterByName('cat');
        console.log(categoryName);
        if (categoryName === undefined) {

            //use the category from the class assigned to the input element
            //... this is strange but I guess it works. If there is no class
            // then there is no category.

            if (ev && ev.target && ev.target.attributes && ev.target.attributes.class && ev.target.attributes.class.value) {
                categoryName = ev.target.attributes.class.value;
            }

        }

        var version = $(".version-search-select").val();

        var filters = GetSearchFilters(categoryName);

        var newUri = "/search";
        newUri = updateQueryString("q", query, newUri);
        if (filters.category !== "search-input" && filters.category !== "search-submit") {
            newUri = updateQueryString("cat", filters.category, newUri);
        }
        if (filters.category === "docs") {
            newUri = updateQueryString("cat", "documentation", newUri);
        }
        newUri = updateQueryString("fid", filters.forumId, newUri);
        newUri = updateQueryString("order", filters.order, newUri);
        newUri = updateQueryString("noreplies", filters.noreplies, newUri);
        newUri = updateQueryString("replies", filters.replies, newUri);
        newUri = updateQueryString("solved", filters.solved, newUri);
        newUri = updateQueryString("unsolved", filters.unsolved, newUri);

                newUri = updateQueryString("v", version, newUri);

        window.location = newUri;
    }
}

function GetSearchFilters(cssClass) {
    var filters = {
        category: undefined,
        forumId: undefined,
        order: undefined,
        solved: undefined,
        unsolved: undefined,
        replies: undefined,
        noreplies: undefined
    };
    if (cssClass === undefined) {
        filters.category = undefined;
    } else {
        var category = cssClass.replace("-search-input valid", "").replace("-search-input", "");
        filters.category = category;
    }

    var forumId = $('#selectCategory').val();
    if (forumId) {
        forumId = forumId.replace(/([A-Za-z])\w+/g, "");
    }

    filters.forumId = forumId;
    filters.order = getParameterByName('order');
    filters.solved = getParameterByName('solved');
    filters.unsolved = getParameterByName('unsolved');
    filters.replies = getParameterByName('replies');
    filters.noreplies = getParameterByName('noreplies');

    return filters;
}

var projectSearch = _.debounce(function (ev) {
    var term = $('.project-search-input').val();
    var defaultListing = $('.projects-default-listing');
    var searchListing = $('.projects-search-listing');
    var searchTable = $('.projects-default-listing');

    if (term.length === 0) {
        defaultListing.show();
        searchListing.empty();
    }

    if (term.length > 3) {
        var url = "/umbraco/api/OurSearch/GetProjectSearchResults";
        var template = _.template(
            $("script.search-item-project").html()
        );

        //get search from server
        $.ajax({
            dataType: "json",
            url: url,
            data: { term: term.replace(":", " ") },
            success: function (response) {

                //toggle and empty box
                defaultListing.hide();
                searchListing.empty();

                if (response.items.length === 0) {
                    var linkhtml = '<p>No results found</p>';
                    searchListing.append(linkhtml);
                }

                //iterate
                _.each(response.items, function (item) {

                    var image = (item.Fields.image && item.Fields.image.length > 0) ? item.Fields.image : null;
                    var itemData = {
                        nodeName: item.Fields.nodeName,
                        body: _.escape(item.Fields.body.substring(0, 150)),
                        url: item.Fields.url,
                        karma: item.Fields.karma,
                        downloads: item.Fields.downloads,
                        versions: item.Fields.versions,
                        image: image
                    };


                    searchListing.append(
                        template(itemData)
                    );
                });

            }
        });
    }
}, 300);


var docsSearch = _.debounce(function (ev) {

    var term = this.value;
    var version = $('.version-search-select').find(":selected").val();

    var defaultListing = $('.docs-default-listing');
    var searchListing = $('.docs-search-listing');

    if (term.length === 0) {
        defaultListing.show();
        searchListing.empty();
    }

    if (term.length > 3) {
        var url = "/umbraco/api/OurSearch/GetDocsSearchResults";
        var template = _.template(
            $("script.search-item-docs").html()
        );

        //fade out existing searchresults
        searchListing.addClass('fadeResultOut');

        //get search from server
        $.ajax({
            dataType: "json",
            url: url,
            data: { term: term, version: version },
            success: function (response) {

                //toggle and empty box
                defaultListing.hide();
                searchListing.empty();

                //Fade in new search results
                setTimeout(function () {
                    searchListing.removeClass('fadeResultOut');
                }, 1000);

                if (response.items.length === 0) {
                    var linkhtml = '<li>No documentation found</li>';
                    searchListing.append(linkhtml);
                }

                //iterate
                _.each(response.items, function (item) {
                    var itemData = {
                        name: item.Fields.nodeName,
                        body: _.escape(item.Fields.body.substring(0, 150)),
                        url: item.Fields.url
                    };

                    searchListing.append(
                        template(itemData)
                    );
                });

            }
        });
    }
}, 300);


var forumSearch = _.debounce(function (ev) {
    var term = this.value;
    var defaultListing = $('.forum-default-listing');
    var searchListing = $('.forum-search-listing');
    var searchTable = $('.table-striped');

    console.log(term);

    if (term.length === 0) {
        defaultListing.show();
        searchListing.empty();
    }

    if (term.length > 3) {
        var url = "/umbraco/api/OurSearch/GetForumSearchResults";
        var template = _.template(
            $("script.search-item-forum").html()
        );

        //fade out existing searchresults
        searchTable.addClass('fadeResultOut');

        var filters = GetSearchFilters("forum");

        //get search from server
        $.ajax({
            dataType: "json",
            url: url,
            data: {
                term: term,
                forum: filters.forumId
            },
            success: function (response) {

                //toggle and empty box
                defaultListing.hide();
                searchListing.empty();

                //Fade in new search results
                setTimeout(function () {
                    searchTable.removeClass('fadeResultOut');
                }, 1000);


                if (response.items.length === 0) {
                    var linkhtml = '<tr><td>No threads found</td></tr>';
                    searchListing.append(linkhtml);
                }

                //iterate
                _.each(response.items, function (item) {
                    var itemData = {
                        name: item.Fields.nodeName,
                        url: item.Fields.url,

                        hasAnswer: item.Fields.solved === 1,
                        replies: item.Fields.replies,

                        body: item.Fields.body.substring(0, 150),

                        forumUrl: item.Fields.parentId,
                        forumName: "Category",

                        updateDate: item.Fields.updateDate
                    };

                    searchListing.append(
                        template(itemData)
                    );
                });

            }
        });
    }
}, 300);


var globalSearch = _.debounce(function (ev) {
    var term = this.value;

    if (term.length === 0) {
        $('.search-all').removeClass('open');
    }

    if (term.length > 3) {
        var url = "/umbraco/api/OurSearch/GetGlobalSearchResults";
        var template = _.template(
            $("script.search-item-template").html()
        );

        //get search from server
        $.ajax({
            dataType: "json",
            url: url,
            data: { term: term },
            success: function (response) {
                //toggle and empty box
                $('.search-all').addClass('open');
                $('.search-all ul').empty();

                //iterate
                _.each(response.items, function (item) {

                    //append as templated html
                    var type = item.Fields.nodeTypeAlias;
                    var icon = "icon-Chat";

                    if (type === "documentation") {
                        icon = "icon-Book-alt";
                    }
                    if (type === "project") {
                        icon = "icon-Box";
                    }

                    var itemData = { icon: icon, name: item.Fields.nodeName, description: _.escape(item.Fields.body.substring(0, 100)), url: item.Fields.url };
                    $('.search-all ul').append(
                        template(itemData)
                    );
                });

                if (response.items.length > 0) {
                    var linkhtml = '<li class="search-view-all"><a href="/search?q=' + response.term + '">View All results</a></li>';
                    $('.search-all ul').append(linkhtml);
                } else {
                    $('.search-all ul').append("<li>No results</li>");
                }

            }
        });
    }
}, 300);

jQuery(document).ready(function () {

    //docs search form
    $('.docs-search-input')
        .keydown(redirectToSearch)
        .keyup(docsSearch);

    //forum search form
    $('.forum-search-input')
        .keydown(redirectToSearch)
        .keyup(forumSearch);

    // Frontpage search form
    $('.search-input')
        .keydown(redirectToSearch)
        .keyup(globalSearch);
    $('.search-submit')
        .click(redirectToSearch);

    //project search form
    $('.project-search-input')
        .keydown(redirectToSearch)
        .keyup(projectSearch);

    $('.search-big input[type=text]')
        .keydown(redirectToSearch);

    $('#search-options input[type=checkbox]').click(function () {
        var newUri;
        if (this.name === "order") {
            if (this.name === "order") {
                var orderValue = this.value;
                if (this.checked === false) {
                    orderValue = undefined;
                }
                newUri = updateQueryString("order", orderValue, window.location.href);
            }
        } else {
            newUri = updateQueryString(this.name, this.checked, window.location.href);
            newUri = updateQueryString("page", 1, newUri);
        }
        console.log(newUri);
        window.location = newUri;
    });

    initCheckbox('replies');
    initCheckbox('noreplies');
    initCheckbox('solved');
    initCheckbox('unsolved');
    initCheckbox('order');

});

// From: http://stackoverflow.com/a/11654596/5018
function updateQueryString(key, value, url) {
    if (!url) url = window.location.href;
    var re = new RegExp("([?&])" + key + "=.*?(&|#|$)(.*)", "gi"),
        hash;

    if (re.test(url)) {
        if (typeof value !== 'undefined' && value !== null)
            return url.replace(re, '$1' + key + "=" + value + '$2$3');
        else {
            hash = url.split('#');
            url = hash[0].replace(re, '$1$3').replace(/(&|\?)$/, '');
            if (typeof hash[1] !== 'undefined' && hash[1] !== null)
                url += '#' + hash[1];
            return url;
        }
    }
    else {
        if (typeof value !== 'undefined' && value !== null) {
            var separator = url.indexOf('?') !== -1 ? '&' : '?';
            hash = url.split('#');
            url = hash[0] + separator + key + '=' + value;
            if (typeof hash[1] !== 'undefined' && hash[1] !== null)
                url += '#' + hash[1];
            return url;
        }
        else
            return url;
    }
}

// From: http://stackoverflow.com/a/901144/5018
function getParameterByName(name) {
    name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
    var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
        results = regex.exec(location.search);
    return results === null ? undefined : decodeURIComponent(results[1].replace(/\+/g, " "));
}

function initCheckbox(name) {
    if (getParameterByName(name) === "true") {
        $('#search-options input[name=' + name + ']').prop('checked', true);
    }
    else {
        var value = getParameterByName(name);
        $('#search-options input[value=' + value + ']').prop('checked', true);
    }
}
