$(function() {
    var numberOfMessages = 4;

    //Emoji-lib - allow :-) to be parsed
    //Rather than just :smile:
    emojione.ascii = true;

    // Reference the auto-generated proxy for the hub.
    var gitter = $.connection.gitterHub;

    //Currently unused - but ready to work
    gitter.client.prescenceEvent = function(data) {
        console.log('User Presence', data);
    };

    //Currently unused - but ready to work
    gitter.client.roomEvent = function(data) {
        console.log('Room event', data);
    };

    //Currently unused - but ready to work
    gitter.client.userEvent = function(data) {
        console.log('User event', data);
    };

    gitter.client.chatMessage = function(data) {

        //If the payload is 'remove' then user has deleted message
        if (data.operation === 'remove') {
            //Check if the current message being displayed in the DOM
            //Matches the ID of this item we want to remove
            if (checkMessageIsInDom(data.message.id)) {
                //If so call the SignalR server event to get last message
                //Which will then call back to fetchedChatMessage
                $.connection.gitterHub.server.getLatestChatMessages(data.room, numberOfMessages);
            }
        }

        //Operation is 'create' so just update the DOM instantly
        if (data.operation === 'create') {

            //Need to remove the first DOM item in the container
            //As its the oldest
            $(".gitter-room[data-room='" + data.room + "'] .gitter-message:first-child").remove();

            //Insert new item in the container at the bottom
            $(".gitter-room[data-room='" + data.room + "']").append(renderMustacheTemplate(data.message));
        }

        //Operation is 'update' - need to check if its the current message being displayed
        if (data.operation === 'update') {
            if (checkMessageIsInDom(data.message.id)) {
                //If so then update the DOM item
                $(".gitter-room[data-room='" + data.room + "'] .gitter-message[data-message-id='" + data.message.id + "']")
                    .replaceWith(renderMustacheTemplate(data.message));
            }
        }

    };

    gitter.client.fetchedChatMessage = function(data) {

        //On init load of SignalR, it's pinged from client to server to fetch latest message
        //Via Gitter API which returns a collection of messages (even though its one message)
        var html = renderMultipleMustacheTemplate(data.messages);

        $(".gitter-room[data-room='" + data.room + "']").html(html);
    };

    function checkMessageIsInDom(messageId) {
        var foundInDom = $(".gitter-message[data-message-id='" + messageId + "']");
        return foundInDom.length > 0;
    }

    function renderMultipleMustacheTemplate(data) {

        var innerTemplate = $('#gitter-chat-template').html();
        var outerTemplate = $('#gitter-chat-messages-template').html();

        var multiHtml = Mustache.render(outerTemplate, data, {
            message: innerTemplate
        });

        //Re-render HTML with emjoi's
        //var emjoiHtml = emojione.shortnameToUnicode(multiHtml);
        var emjoiHtml = emojione.toImage(multiHtml);
        return emjoiHtml;
    };

    function renderMustacheTemplate(data) {

        //Render mustache js template with JSON data
        //This is used for single messages coming from the realtime api
        //Where the main message payload is a subobject stored in 'model'
        var template = $('#gitter-chat-template').html();
        var html = Mustache.render(template, data);

        //Re-render HTML with emjoi's
        //var emjoiHtml = emojione.shortnameToUnicode(html);
        var emjoiHtml = emojione.toImage(html);
        return emjoiHtml;
    }
});