var uPowers = function () {
    return {
        LikeTopic: function (s_topicId) {
            $.get("/base/uPowers/Action/LikeTopic/" + s_topicId + ".aspx");
        },
        DeleteTopic: function (s_Id) {
            //This is secured on the serverside, so don't even bother fuckers...
            $.get("/base/uForum/DeleteTopic/" + s_Id + ".aspx");
        },
        MoveTopic: function (s_Id, f_id) {
            //This is secured on the serverside, so don't even bother fuckers...

            $.get("/base/uForum/MoveTopic/" + s_Id + "/" + f_id + ".aspx",
      function (data) {
          window.location = jQuery("value", data).text();
      });

        },
        LikeComment: function (s_Id) {
            $.get("/base/uPowers/Action/LikeComment/" + s_Id + ".aspx");
        },
        DeleteComment: function (s_Id) {
            //This is secured on the serverside, so don't even bother fuckers...
            $.get("/base/uForum/DeleteComment/" + s_Id + ".aspx");
        },
        MarkCommentAsSpam: function (s_Id) {
            //This is secured on the serverside, so don't even bother fuckers...
            $.get("/base/uForum/MarkCommentAsSpam/" + s_Id + ".aspx");
        },
        MarkTopicAsSpam: function (s_Id) {
            //This is secured on the serverside, so don't even bother fuckers...
            $.get("/base/uForum/MarkTopicAsSpam/" + s_Id + ".aspx");
        },

        EventSignUp: function (s_link, s_Id) {

            $.get("/base/uEvents/Toggle/" + s_Id + ".aspx",
      function (data) {
          window.location = s_link + "?done=true";
      });
        },

        WikiUp: function (s_nodeId) {
            $.get("/base/uPowers/Action/WikiUp/" + s_nodeId + ".aspx");
        },
        WikiDelete: function (s_Id) {
            //This is secured on the serverside, so don't even bother fuckers...
            $.get("/base/uWiki/Delete/" + s_Id + ".aspx");
        },
        WikiMove: function (s_Id, s_target) {
            //This is secured on the serverside, so don't even bother fuckers...
            $.get("/base/uWiki/Move/" + s_Id + "/" + s_target + ".aspx",
      function (data) {
          top.location = jQuery("value", data).text();
      });
        },
        ProjectUp: function (s_nodeId, comment) {
            $.get("/base/uPowers/Action/ProjectUp/" + s_nodeId + ".aspx");
        },

        ProjectApproval: function (s_nodeId) {
            $.post("/base/uPowers/Action/ProjectApproval/" + s_nodeId + ".aspx");
        },

        SolvesProblem: function (s_nodeId) {
            $.get("/base/uPowers/Action/TopicSolved/" + s_nodeId + ".aspx");
        },
        BlockMember: function (s_memberId) {
            //This is secured on the serverside, so don't even bother fuckers...
            $.get("/base/Community/BlockMember/" + s_memberId + ".aspx");
        },
        UnBlockMember: function (s_memberId) {
            //This is secured on the serverside, so don't even bother fuckers...
            $.get("/base/Community/UnBlockMember/" + s_memberId + ".aspx");
        }
    };
} ();

function updateScore(point, className, text, obj){
  var p = obj;
  jQuery('a', p).hide();
  var score = jQuery('span a', p);
  score.text( parseInt(score.text()) + point).show();

  p.append("<div class='" + className +"'>" + text + "</div>");
  p.effect('highlight');
}

function CreateModal(obj, header, description, voteFunc, scoreFunc){
  var id = obj.attr("rel"); 
  var m = jQuery("#votingModal");
  var h = jQuery("h3", m); 
  var p = jQuery("p", m);
  var t = jQuery("textarea", m);   
  var b = jQuery("input", m);
  var em = jQuery("em.counter", m);
  
  em.html("50");
  h.html(header);
  p.html(description);

  b.attr("disabled", true).unbind("click").click( function(){
    voteFunc( id, t.val() );
    jQuery.modal.close();
    scoreFunc( obj.parent() );
  });
  

  t.val("").keyup( function(event){
    if(t.val().length < 50){
      em.html( 50 - t.val().length);
      b.attr("disabled", true);
    }
    else{
      em.html("0");
        b.attr("disabled", false);
    }
  });

  m.modal( {position: ["100px",]} );
  
  t.focus();
}


jQuery(document).ready(function () {

    jQuery("a#eventSignup").click(function () {
        uPowers.EventSignUp(jQuery(this).attr("href"), jQuery(this).attr("rel"));
        return false;
    });

    jQuery("a.LikeTopic").click(function () {
        uPowers.LikeTopic(jQuery(this).attr("rel"));

        updateScore(1, 'like', 'You rock!', jQuery(this).parent());

        return false;
    });
    jQuery("a.DisLikeTopic").click(function () {

        CreateModal(jQuery(this), "Constructive criticism", "Please add a short comment so the author knows why you do not like this topic", uPowers.DisLikeTopic
    , function (obj) { updateScore(-1, 'dislike', "Don't like", obj) });

        return false;
    });

    jQuery("a.LikeComment").click(function () {
        uPowers.LikeComment(jQuery(this).attr("rel"));
        updateScore(1, 'like', 'You rock!', jQuery(this).parent());
        return false;
    });

    jQuery("a.DisLikeComment").click(function () {
        CreateModal(jQuery(this), "Constructive criticism", "Please add a short comment so the author knows why you do not like this comment", uPowers.DislikeComment
    , function (obj) { updateScore(-1, 'dislike', "Don't like", obj) });

        return false;
    });

    jQuery("a.DeleteComment").click(function () {
        if (confirm("Do you really want to delete this comment?")) {
            var link = jQuery(this);
            var comment = jQuery("#comment" + link.attr("rel"));

            uPowers.DeleteComment(link.attr("rel"));
            comment.hide("slow");

        }

        return false;
    });


    jQuery("a.MarkCommentAsSpam").click(function () {
        if (confirm("Do you really want mark this comment as spam?")) {
            var link = jQuery(this);
            var comment = jQuery("#comment" + link.attr("rel"));

            uPowers.MarkCommentAsSpam(link.attr("rel"));
            comment.hide("slow");

        }

        return false;
    });

    jQuery("a.DeleteTopic").click(function () {
        if (confirm("Do you really want to delete this entire topic and all comments?")) {
            var link = jQuery(this);
            var topic = jQuery("#forum");

            uPowers.DeleteTopic(link.attr("rel"));

            topic.after("<div class='success' style='text-align: center;'><h3>Topic  deleted</h3></div>");
            topic.hide();

        }

        return false;
    });
    
    jQuery("a.MarkTopicAsSpam").click(function () {
        if (confirm("Do you really want to mark this topic as spam?")) {
            var link = jQuery(this);
            var topic = jQuery("#forum");

            uPowers.MarkTopicAsSpam(link.attr("rel"));

            topic.after("<div class='success' style='text-align: center;'><h3>Topic marked as spam</h3></div>");
            topic.hide();

        }

        return false;
    });

    jQuery("a.MoveToForum").click(function () {
        if (confirm("Do you really want to move this topic?")) {
            var link = jQuery(this);
            var topicID = jQuery("#MoveTopic").attr("rel");

            uPowers.MoveTopic(topicID, link.attr("rel"));
        }
        return false;
    });

    jQuery("#ToggleMoveList").click(function () {
        var list = jQuery("#moveList");
        list.toggle();
        return false;
    });


    jQuery("a.WikiUp").click(function () {
        uPowers.WikiUp(jQuery(this).attr("rel"));
        updateScore(1, 'like', 'You rock!', jQuery(this).parent());
        return false;
    });

    jQuery("a.ProjectApproval").click(function () {
        uPowers.ProjectApproval(jQuery(this).attr("rel"));
        updateScore(15, 'like', 'Approved', jQuery(this).parent());
        return false;
    });

    jQuery("a.WikiDown").click(function () {

        CreateModal(jQuery(this), "Constructive criticism", "Please add a short comment so the author knows why you do not like this wiki page", uPowers.WikiDown
    , function (obj) { updateScore(-1, 'dislike', "Don't like", obj) });

        return false;
    });

    //admin only..
    jQuery("a.WikiDelete").click(function () {

        if (confirm("Do you really want to delete this page?")) {
            uPowers.WikiDelete(jQuery(this).attr("rel"));
        }

        return false;
    });
    jQuery("a.WikiMove").click(function () {
        //uPowers.WikiMove(jQuery(this).attr("rel"));
        jQuery.modal("<h3>Move wiki page</h3><iframe src='/WikiMoveDialog.aspx?target=" + jQuery(this).attr("rel") + "' id='wikimovedialog'></iframe>", { position: ["100px", ], overlayClose: true, closeHTML: '<a href="#" id="modalCloseButton" title="Close">close</a>' });
        return false;
    });

    jQuery("a.WikiMoveDo").click(function () {

        var link = jQuery(this);
        uPowers.WikiMove(link.attr("id"), link.attr("rel"));
        return false;
    });

    jQuery("a.ProjectUp").click(function () {
        uPowers.ProjectUp(jQuery(this).attr("rel"));
        updateScore(1, 'like', 'You rock!', jQuery(this).parent());
        return false;
    });
    jQuery("a.ProjectDown").click(function () {

        CreateModal(jQuery(this), "Constructive criticism", "Please add a short comment so the author knows why you do not like this project", uPowers.ProjectDown
    , function (obj) { updateScore(-1, 'dislike', "Don't like", obj) });

        return false;
    });

    jQuery("a.TopicSolver").click(function () {
        uPowers.SolvesProblem(jQuery(this).attr("rel"));
        jQuery("a.TopicSolver").hide();
	
	//removed as this does not work for topic solving.
        //updateScore(100, 'like', 'Solved', jQuery(this).parent());
	// replaced with this as this will set the topic to solved but not effect the visual score
	
	var p = jQuery(this).parent();
    	var post = jQuery(this).closest('li.postComment');
	post.addClass('postSolution');
  	p.append("<div class='like'>Solved</div>");
  	p.effect('highlight');
        return false;
    });

    jQuery(".voting a.history").click(function () {
        var key = jQuery(this).attr("rel").split(",")[0];
        var type = jQuery(this).attr("rel").split(",")[1];

        $.get("/html/votinghistory?id=" + key + "&type=" + type,
      function (data) {
          jQuery.modal("<div><h3>Voting history</h3><p>Shows who have voted, and the reasoning for voting</p>" + data + "</div>", { position: ["100px", ], overlayClose: true, closeHTML: '<a href="#" id="modalCloseButton" title="Close">close</a>' });
      });
        return false;
    });

    jQuery("a.noVote").click(function () {
        jQuery.modal("<h3>You cannot vote yet</h3><p>You need at least <em>70 karma points</em> to be able to rate items on our.umbraco.org</p><p>You gain karma points every time you do something constructive, like answering topics on the forum, or starting new ones or publishing your work as a project</p></div>", { position: ["100px", ], overlayClose: true, closeHTML: '<a href="#" id="modalCloseButton" title="Close">close</a>' });
        return false;
    });

    jQuery("a.blockMember").click(function () {
        if (confirm("Do you really want to block this member?")) {
            var blockLink   = jQuery(this);
            var unblockLink = jQuery("a.unblockMember");

            uPowers.BlockMember(blockLink.attr("rel"));
            blockLink.parent("li").hide();
            unblockLink.parent("li").show();

        }
        return false;
    });

    jQuery("a.unblockMember").click(function () {
        if (confirm("Do you really want to un-block this member?")) {
            var unblockLink = jQuery(this);
            var blockLink   = jQuery("a.blockMember");

            uPowers.UnBlockMember(unblockLink.attr("rel"));
            unblockLink.parent("li").hide();
            blockLink.parent("li").show();

        }
        return false;
    });

});