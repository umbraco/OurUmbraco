function AddTagToProject(projectid,tag){
    $.get("/umbraco/api/Community/AddTag/?projectId=" + projectid + "&group=project&tag=" + tag);
}

function RemoveTagFromProject(projectid,tag){
  $.get("/umbraco/api/Community/RemoveTag/?projectId=" + projectid + "&group=project&tag=" + tag);
}

function VerifyFile(fileId)
{
    $.post("/umbraco/api/Wiki/VerifyFile/?fileId=" + fileId);
}

jQuery(document).ready(function () {
    $("#fileArchiveLink").click(function (e) {
        $("#fileArchive").toggle('fast');
    });


    $(".verifyWikiFile").click(function (e) {
        VerifyFile(jQuery(this).attr("rel"));
        jQuery(this).hide("slow");
    });

    $("a.viewFullCompatibilityDetails").click(function () {
        if ($(this).text() == "View Details") {
            $("#compatLoading").show();
            
            var key = $(this).attr("rel").split(",");
            $.get("/html/versioncompatibilityreport?fileId=" + key[0] + "&packageId=" + key[1],
            function (data) {
                $("#compatLoading").hide();
                $("#compatAjaxDetails").html(data);
                $(".compatSummary").hide();
                $(".compatDetails").show('slow');
            });
            $(this).text("Hide Details");
        } else {
            $(".compatDetails").hide();
            $(".compatSummary").show('slow');
            $(this).text("View Details");
        }
        return false;
    });

    $("a.viewFullCompatibilityDetails").trigger("click");

    $("a.compatibilityReport").click(function () {
        var key = $(this).attr("rel").split(",");

        $.get("/html/versioncompatibility?fileId=" + key[0] + "&packageId=" + key[1],
      function (data) {
          jQuery.modal("<div><h3>I've tried this!</h3><p>Have you tried this package? Help others by letting them know if it's compatible with their version of Umbraco!</p><div class=\"versionForm\">" + data + "</div></div>", { position: ["100px", ], overlayClose: true, closeHTML: '<a href="#" id="modalCloseButton" title="Close">close</a>' });
          $('#reportCompatibility').click(function () {
              var compatArr = "";
              $(".projectCompatList tr").each(function (index) {
                  compatArr += $(this).find("span").text() + "^" + $(this).find("input[type='radio']:checked").val() + ",";
              });
              $.get("/base/deli/CompatibilityReport/" + $("#packageId").val() + "/" + $("#packageFileId").val() + "/" + compatArr.substr(0, compatArr.length - 1) + ".aspx", function (data) {
                  $(".versionForm").html("<br/><br/><p><strong>Thankyou for taking the time to provide your feedback</strong></p>");
                  if ($(".compatDetails:visible")) {
                      $(".compatDetails").hide();
                      $("#compatLoading").show();
                      $.get("/html/versioncompatibilityreport?fileId=" + key[0] + "&packageId=" + key[1],
                        function (data) {
                            $("#compatLoading").hide();
                            $("#compatAjaxDetails").html(data);
                            $(".compatDetails").show('slow');
                        });
                  }
              });
              return false;
          });



      });
        return false;
    });

});