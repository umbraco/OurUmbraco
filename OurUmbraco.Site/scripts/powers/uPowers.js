var uPowers = function () {
    return {
        LikeTopic: function (s_topicId) {
            $.get("/umbraco/api/Powers/Action/?alias=LikeTopic&pageId=" + s_topicId);
        },
        DeleteTopic: function (s_Id) {
            
            $.get("/umbraco/api/Forum/DeleteTopic/?topicId=" + s_Id);
        },
        MoveTopic: function (s_Id, f_id) {
            

            $.get("/umbraco/api/Forum/MoveTopic/?topicId=" + s_Id + "&newForumId=" + f_id,
      function (data) {
          window.location = jQuery("value", data).text();
      });

        },
        LikeComment: function (s_Id) {
            $.get("/umbraco/api/Powers/Action/?alias=LikeComment&pageId=" + s_Id);
        },
        DeleteComment: function (s_Id) {
            
            $.get("/umbraco/api/Forum/DeleteComment/?commentId=" + s_Id);
        },
        MarkCommentAsSpam: function (s_Id) {
            
            $.get("/umbraco/api/Forum/MarkCommentAsSpam/?commentId=" + s_Id);
        },
        MarkCommentAsHam: function (s_Id) {
            
            $.get("/umbraco/api/Forum/MarkCommentAsHam/?commentId=" + s_Id);
        },
        MarkTopicAsSpam: function (s_Id) {
            
            $.get("/umbraco/api/Forum/MarkTopicAsSpam/?topicId=" + s_Id);
        },
        MarkTopicAsHam: function (s_Id) {
            
            $.get("/umbraco/api/Forum/MarkTopicAsHam/?topicId=" + s_Id);
        },

        EventSignUp: function (s_link, s_Id) {

            $.get("/umbraco/api/Events/Toggle/?eventId=" + s_Id,
      function (data) {
          window.location = s_link + "?done=true";
      });
        },

        WikiUp: function (s_nodeId) {
            $.get("/umbraco/api/Powers/Action/?alias=WikiUp&pageId=" + s_nodeId);
        },
        WikiDelete: function (s_Id) {
            
            $.get("/umbraco/api/wiki/Delete/?wikiId=" + s_Id);
        },
        WikiMove: function (s_Id, s_target) {
            
            $.get("/umbraco/api/wiki/Move/?wikiId=" + s_Id + "&target=" + s_target,
      function (data) {
          top.location = jQuery("value", data).text();
      });
        },
        ProjectUp: function (s_nodeId, comment) {
            $.get("/umbraco/api/Powers/Action/?alias=ProjectUp&pageId=" + s_nodeId);
        },

        ProjectApproval: function (s_nodeId) {
            $.get("/umbraco/api/Powers/Action/?alias=ProjectApproval&pageId=" + s_nodeId);
        },

        SolvesProblem: function (s_nodeId) {
            $.get("/umbraco/api/Powers/Action/?alias=TopicSolved&pageId=" + s_nodeId);
        },
        UnBlockMember: function (s_memberId) {
            
            $.get("/umbraco/api/Community/UnBlockMember/" + s_memberId);
        },
        DeleteMember: function (s_memberId) {

            $.ajax({
                url: "/umbraco/api/Forum/DeleteMember/?id=" + s_memberId,
                type: 'DELETE',
                success: function (result) {
                    // Do something with the result
                }
            });
        },
        BlockMember: function (s_memberId) {

            $.ajax({
                url: "/umbraco/api/Forum/BlockMember/?id=" + s_memberId,
                type: 'POST',
                success: function (result) {
                    // Do something with the result
                }
            });
        },
        UnblockMember: function (s_memberId) {

            $.ajax({
                url: "/umbraco/api/Forum/UnblockMember/?id=" + s_memberId,
                type: 'POST',
                success: function (result) {
                    // Do something with the result
                }
            });
        },
        DeleteMemberPlus: function (s_memberId) {

            $.ajax({
                url: "/umbraco/api/Forum/DeleteMemberPlus/?id=" + s_memberId,
                type: 'DELETE',
                success: function (result) {
                    // Do something with the result
                }
            });
        },
        ApproveMember: function (s_memberId) {

            $.ajax({
                url: "/umbraco/api/Forum/ApproveMember/?id=" + s_memberId,
                type: 'POST',
                success: function (result) {
                    jQuery(".karma-points").text(result);
                }
            });
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


    jQuery("a.MarkCommentAsHam").click(function () {
        if (confirm("Do you really want mark this comment as ham?")) {
            var link = jQuery(this);
            var comment = jQuery("#comment" + link.attr("rel"));

            uPowers.MarkCommentAsHam(link.attr("rel"));
            link.hide();
            comment.find(".spamNotice").first().hide("fast");

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

    jQuery("a.MarkTopicAsHam").click(function () {
        if (confirm("Do you really want to mark this topic as ham?")) {
            var link = jQuery(this);
            var topic = jQuery("#forum");

            uPowers.MarkTopicAsHam(link.attr("rel"));

            link.hide();
            topic.after("<div class='success' style='text-align: center;'><h3>Topic marked as ham</h3></div>");
            topic.find(".spamNotice").first().hide("fast");

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
        jQuery.modal("<h3>You cannot vote yet</h3><p>You need at least <em>70 karma points</em> to be able to rate items on our.umbraco.com</p><p>You gain karma points every time you do something constructive, like answering topics on the forum, or starting new ones or publishing your work as a project</p></div>", { position: ["100px", ], overlayClose: true, closeHTML: '<a href="#" id="modalCloseButton" title="Close">close</a>' });
        return false;
    });


    jQuery("a.delete-member").click(function () {
        if (confirm("Do you really want to DELETE this member?")) {
            var deleteLink = jQuery(this);
            
            uPowers.DeleteMember(deleteLink.attr("rel"));
            deleteLink.hide();
        }
        return false;
    });

    jQuery("a.block-member").click(function () {
        if (confirm("Do you really want to BLOCK this member?")) {
            var blockLink = jQuery(this);
            
            uPowers.BlockMember(blockLink.attr("rel"));
            blockLink.hide();
        }
        return false;
    });

    jQuery("a.unblock-member").click(function () {
        if (confirm("Do you really want to UNBLOCK this member?")) {
            var blockLink = jQuery(this);
            
            uPowers.UnblockMember(blockLink.attr("rel"));
            blockLink.hide();
            jQuery("strong.member-blocked").hide();
        }
        return false;
    });
    
    jQuery("a.delete-member-plus").click(function () {
        if (confirm("Do you really want to DELETE this member and ALL of their created topics and comments?")) {
            var deleteLink = jQuery(this);
            
            uPowers.DeleteMemberPlus(deleteLink.attr("rel"));
            deleteLink.hide();
        }
        return false;
    });

    jQuery("a.approve-member").click(function () {
        if (confirm("Do you really want to approve this member?")) {
            var approveLink = jQuery(this);
            uPowers.ApproveMember(approveLink.attr("rel"));
            approveLink.hide();
        }
        return false;
    });

});