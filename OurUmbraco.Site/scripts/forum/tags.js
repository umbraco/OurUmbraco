    $(function() {
        var choice;
        var slider = $("#slider").slider({
            min: 1,
            max: 5,
            step: 1,
            change: function(ev, ui) {
                if (choice) {
                    choice.data.size = ui.value;

                    $(choice.element)
                        .removeClass("size-1 size-2 size-3 size-4 size-5")
                        .addClass("size-" + choice.data.size);

                    showSlider();
                }
            }
        }).children().on("focus", function(e) {
            e.preventDefault();
        }).end();
            
        hideSlider(true);

        function hideSlider(now) {
            slider.hide();
        }

        function showSlider() {
            if (choice) {
                slider.show();
                slider.position({
                    my: "center top",
                    at: "center bottom",
                    of: choice.element,
                    collision: "none",
                    offset: "0 5"
                });
                if (slider.slider("option", "value") != choice.data.size) {
                    slider.slider("option", "value", choice.data.size);
                }
            }
        }


        $("#tags")
            .on("choice-begin-edit", function(e, data) {
                //$("input", data.element)[0].focus();
            })
            .on("choice-selected", function(e, data) {
                choice = data;
                showSlider();
            })
            .on("choice-deselected", function(e, data) {
                choice = null;
                hideSlider();
            })
            .on("choice-keydown", function(e, data) {
                if (e.which == 38 || e.which == 40) {
                    var inc = e.which == 38 ? 1 : -1;

                    data.data.size =
                        Math.max(1, Math.min(5, data.data.size + inc));
                    slider.slider("option", "value", data.data.size);
                    e.preventDefault();
                }

            })
            .on("choice-created", function(e, data) {
                data.data.size = 3;
                $(data.element)
                    .addClass("size-" + data.data.size);
            })
            .select2();


        $("#tag-container").on("sortstart", function(e, data) {
            if (choice) hideSlider();
        }).on("sortstop", function() {
            showSlider();
        });

        slider.add(slider.children()).attr("unselectable", "on");
    })
