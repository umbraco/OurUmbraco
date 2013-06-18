$(function () {
    var choice;
    $("#tags")
        .on("choice-begin-edit", function (e, data) {
            //$("input", data.element)[0].focus();
        })
        .on("choice-selected", function (e, data) {
            choice = data;
            // showSlider();
        })
        .on("choice-deselected", function (e, data) {
            choice = null;
            //hideSlider();
        })
        .select2({ tags: ["red", "green", "blue"], tokenSeparators: [",", " "] });
})