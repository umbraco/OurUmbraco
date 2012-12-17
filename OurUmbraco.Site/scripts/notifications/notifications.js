function SubscribeToForum(forumId){
	$.get("/base/Notifications/SubscribeToForum/" + forumId + ".aspx");

	jQuery(".SubscribeForum").hide("slow");
	jQuery(".UnSubscribeForum").show("slow");
}

function UnSubscribeFromForum(forumId){
	$.get("/base/Notifications/UnSubscribeFromForum/" + forumId + ".aspx");

	jQuery(".UnSubscribeForum").hide("slow");
	jQuery(".SubscribeForum").show("slow");
}


function SubscribeToForumTopic(topicId){
	$.get("/base/Notifications/SubscribeToForumTopic/" + topicId + ".aspx");

	jQuery(".SubscribeTopic").hide("slow");
	jQuery(".UnSubscribeTopic").show("slow");
}

function UnSubscribeFromForumTopic(topicId){
	$.get("/base/Notifications/UnSubscribeFromForumTopic/" + topicId + ".aspx");

	jQuery(".UnSubscribeTopic").hide("slow");
	jQuery(".SubscribeTopic").show("slow");
}


function NotificationTopicUnsubscribe(obj, topicId)
{
	$.get("/base/Notifications/UnSubscribeFromForumTopic/" + topicId + ".aspx");
	obj.parent().hide("slow");
}

function NotificationForumUnsubscribe(obj, forumId)
{
	$.get("/base/Notifications/UnSubscribeFromForum/" + forumId + ".aspx");
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