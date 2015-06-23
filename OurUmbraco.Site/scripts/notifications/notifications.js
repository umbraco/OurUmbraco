function SubscribeToForum(forumId){
    $.get("/umbraco/api/Notifications/SubscribeToForum/?forumId=" + forumId);

	jQuery(".SubscribeForum").hide("slow");
	jQuery(".UnSubscribeForum").show("slow");
}

function UnSubscribeFromForum(forumId){
    $.get("/umbraco/api/Notifications/UnSubscribeFromForum/?forumId=" + forumId);

	jQuery(".UnSubscribeForum").hide("slow");
	jQuery(".SubscribeForum").show("slow");
}


function SubscribeToForumTopic(topicId){
	$.get("/umbraco/api/Notifications/SubscribeToForumTopic/?topicId=" + topicId);

	jQuery(".SubscribeTopic").hide("slow");
	jQuery(".UnSubscribeTopic").show("slow");
}

function UnSubscribeFromForumTopic(topicId){
    $.get("/umbraco/api/Notifications/UnSubscribeFromForumTopic/?topicId=" + topicId);

	jQuery(".UnSubscribeTopic").hide("slow");
	jQuery(".SubscribeTopic").show("slow");
}


function NotificationTopicUnsubscribe(obj, topicId)
{
    $.get("/umbraco/api/Notifications/UnSubscribeFromForumTopic/?topicId=" + topicId);
	obj.parent().hide("slow");
}

function NotificationForumUnsubscribe(obj, forumId)
{
    $.get("/umbraco/api/Notifications/UnSubscribeFromForum/?forumId=" + forumId);
	obj.parent().hide("slow");
}

jQuery(document).ready(function(){


	jQuery(".SubscribeForum").click(function(){
		SubscribeToForum(jQuery(this).attr("rel"));
	});

	jQuery(".UnSubscribeForum").click(function(){
		UnSubscribeFromForum(jQuery(this).attr("rel"));
	});

	jQuery(".SubscribeTopic").click(function(){
		SubscribeToForumTopic(jQuery(this).attr("rel"));
	});

	jQuery(".UnSubscribeTopic").click(function(){
		UnSubscribeFromForumTopic(jQuery(this).attr("rel"));
	});


	jQuery(".NotificationForumUnsubscribe").click(function(){
		NotificationForumUnsubscribe(jQuery(this),jQuery(this).attr("rel"));
	});

	jQuery(".NotificationTopicUnsubscribe").click(function(){
		NotificationTopicUnsubscribe(jQuery(this),jQuery(this).attr("rel"));
	});
});