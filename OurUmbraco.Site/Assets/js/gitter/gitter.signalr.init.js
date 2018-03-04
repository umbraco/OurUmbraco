$(function() {
    $.connection.hub.start().done(function() {
        //Get the rooms from sessionStorage - for each room loaded on the page
        //It's roomID will be in the array
        var roomIds = JSON.parse(sessionStorage.getItem("gitterRoomIds") || null);

        for (var i = 0; i < roomIds.length; i++) {
            var roomId = roomIds[i];
            //For each room after signalR has connected
            //Call the server hub event - to the get the last 4 messages
            //So the init page load gets the current messages
            $.connection.gitterHub.server.getLatestChatMessages(roomId, 4);
        }
    });
});