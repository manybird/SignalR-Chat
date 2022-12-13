

$(document).ready(function () {

    //#region connection for hub
    const na1ta = $('#inputNa1ta').val();
    //console.log(`na1ta: ${na1ta}`);

    const caseId = $('#inputCaseId').val();
    const PathBase = $('#hiddenPathBase').val();
    
    var connection = new signalR.HubConnectionBuilder().withUrl(`${PathBase}/chatHub?na1ta=${na1ta}`).build();
    
    var connection = new signalR.HubConnectionBuilder().withUrl(`/chatHub?na1ta=${na1ta}`).build();

    connection.start().then(function () {
        console.log('SignalR connection Started...')
        viewModel.roomList();
        viewModel.userList();
    }).catch(function (err) {
        return console.error(err);
    });
    //on new Message
    connection.on("newMessage", function (messageView) {

        console.log("onNewMessage", messageView);

        var isMine = messageView.from === viewModel.myName();
        var message = new ChatMessage2(messageView, isMine);
        viewModel.chatMessages.push(message);
        //$(".messages-container").animate({ scrollTop: $(".messages-container")[0].scrollHeight }, 1000);
        ScrollToEnd(".messages-container");
    });

    //on new system Message
    connection.on("newSystemMessage", function (messageView) {        
        var message = new ChatMessage2(messageView, false, true);

        console.log('newSystemMessage', message);

        viewModel.chatMessages.push(message);
        ScrollToEnd(".messages-container");
    });

    //on getProfileInfo    
    connection.on("getProfileInfo", function (user) {        
        var displayName = user.fullName;
        var avatar = user.avatar;

        if (displayName == null) displayName = "";
        viewModel.myName(displayName);
        viewModel.myAvatar(avatar);
        viewModel.isUser(user.isUserVisiting);
        viewModel.isLoading(false);
    });

    //on onAdminIdUpdated, after Join chat room
    connection.on("onRoomJoinCompleted", function (room) {
        console.log("onRoomJoinCompleted", room);        
        viewModel.adminId(room.adminId);    
        //viewModel.caseId(room.caseId);
        viewModel.messageHistory();
    });

    //addUser , Other user join to this chat room
    connection.on("addUser", function (user) {
        //console.log("addUser", user);
        viewModel.userAdded(new ChatUser(user.username, user.fullName, user.avatar, user.currentRoom, user.device,user.connectionId));
    });

    //removeUser
    connection.on("removeUser", function (user) {
        viewModel.userRemoved(user.connectionId);
    });

    //addChatRoom
    connection.on("addChatRoom", function (room) {
        viewModel.roomAdded(new ChatRoom(room.id, room.name));
    });

    //updateChatRoom
    connection.on("updateChatRoom", function (room) {
        viewModel.roomUpdated(new ChatRoom(room.id, room.name));
    });

    //removeChatRoom
    connection.on("removeChatRoom", function (id) {
        viewModel.roomDeleted(id);
    });

    //removeChatMessage
    connection.on("removeChatMessage", function (id) {
        viewModel.messageDeleted(id);
    });

    //onError
    connection.on("onError", function (message) {
        viewModel.serverInfoMessage(message);
        $("#errorAlert").removeClass("d-none").show().delay(5000).fadeOut(500);
    });

    //onRoomDeleted
    connection.on("onRoomDeleted", function (message) {
        viewModel.serverInfoMessage(message);
        $("#errorAlert").removeClass("d-none").show().delay(5000).fadeOut(500);

        if (viewModel.chatRooms().length == 0) {
            viewModel.joinedRoom("");
        }
        else {
            // Join to the first room in list
            setTimeout(function () {
                $("#rooms-list li a")[0].click();
            }, 50);
        }
    });

    //#endregion

    
    function AppViewModel(na1ta) {
           
        var self = this;
        self.na1ta = ko.observable("");                
        self.adminId = ko.observable("");
        self.caseId = ko.observable("");

        self.message = ko.observable("");
        self.chatRooms = ko.observableArray([]);
        self.chatUsers = ko.observableArray([]);
        self.chatMessages = ko.observableArray([]);
        self.joinedRoom = ko.observable("");
        self.joinedRoomId = ko.observable("");
        self.serverInfoMessage = ko.observable("");
        self.myName = ko.observable("");
        self.isUser = ko.observable("");
        self.myAvatar = ko.observable("");
        self.isLoading = ko.observable(true);
        self.showAvatar = ko.computed(function () {
            return self.isLoading() == false && self.myAvatar() != null;
        });
                
        self.onEnterPressInMessageBox = function (d, e) {
            if (e.keyCode === 13) {
                //Skip private message
                //self.sendNewMessage(); 
                self.sendToRoom(self.joinedRoom(), self.message());
                self.message("");
            }
            return true;
        };
        self.filter = ko.observable("");
        self.filteredChatUsers = ko.computed(function () {
            if (!self.filter()) {
                return self.chatUsers();
            } else {
                return ko.utils.arrayFilter(self.chatUsers(), function (user) {
                    var dn = user.displayName().toLowerCase();
                    return dn.includes(self.filter().toLowerCase());
                });
            }
        });

        self.sendNewMessage = function () {
            var text = self.message();
            if (text.startsWith("/")) {
                var receiver = text.substring(text.indexOf("(") + 1, text.indexOf(")"));
                var message = text.substring(text.indexOf(")") + 1, text.length);
                self.sendPrivate(receiver, message);
            }
            else {
                self.sendToRoom(self.joinedRoom(), self.message());
            }

            self.message("");
        }

        self.sendToRoom = function (roomName, message) {
            if (roomName.length > 0 && message.length > 0) {
                fetch(PathBase + '/api/Messages/ByAgent/' + self.na1ta(), {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ room: roomName, content: message, caseId:self.caseId() })
                });
            }
        }

        self.sendPrivate = function (receiver, message) {
            if (receiver.length > 0 && message.length > 0) {
                connection.invoke("SendPrivate", receiver.trim(), message.trim());
            }
        }

        self.joinRoom = function (room) {
            self.joinedRoom(room.name());
            self.joinedRoomId(room.id());
            connection.invoke("Join", room.name()).then(function () {
                self.userList();
                //self.messageHistory();
            });
        }

        self.roomList = function () {
            console.log('room list url: ' + u);
            fetch(u).then(response => response.json())
            .then(data => {
                self.chatRooms.removeAll();
                for (var i = 0; i < data.length; i++) {
                    self.chatRooms.push(new ChatRoom(data[i].id, data[i].name));
                }

                if (self.chatRooms().length > 0)
                    self.joinRoom(self.chatRooms()[0]);
            });
        }

        self.userList = function () {
            connection.invoke("GetUsers", self.joinedRoom()).then(function (result) {
                self.chatUsers.removeAll();
                for (var i = 0; i < result.length; i++) {
                    self.chatUsers.push(new ChatUser(result[i].username,
                        result[i].fullName,
                        result[i].avatar,
                        result[i].currentRoom,
                        result[i].device, result[i].connectionId))
                }
            });
        }

        self.handleErrorIfAny = async function (data) {
                        
            var hasError = (data && data.errors);

            if (!hasError) return;
            console.log(data);
            const msg = hasError ? JSON.stringify(data.errors) : "error!";
            alert(msg);

        }

        self.createRoom = function () {
            var roomName = $("#roomName").val();
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ name: roomName,operationMode:0 })
            }).then(response => response.json()).then(self.handleErrorIfAny);
        }

        self.editRoom = function () {
            var roomId = self.joinedRoomId();
            var roomName = $("#newRoomName").val();
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ id: roomId, name: roomName })
            }).then(response => response.json()).then(self.handleErrorIfAny);
        }

        self.deleteRoom = function () {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ id: self.joinedRoomId() })
            });
        }

        self.deleteMessage = function () {
            var messageId = $("#itemToDelete").val();

                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ id: messageId })
            });
        }

        self.messageHistory = function () {
            fetch(url) //+'/5'
                .then(response => response.json())
                .then(data => {
                    self.chatMessages.removeAll();
                    for (var i = 0; i < data.length; i++) {
                        var d = data[i];
                        var isMine = d.from == self.myName();
                        self.chatMessages.push(new ChatMessage(d.id, d.content, d.timestamp, d.from, isMine, d.avatar));
                    }

                    ScrollToEnd(".messages-container");
                    //$(".messages-container").animate({ scrollTop: $(".messages-container")[0].scrollHeight }, 0);
                });
        }

        self.roomAdded = function (room) {
            self.chatRooms.push(room);
        }

        self.roomUpdated = function (updatedRoom) {
            var room = ko.utils.arrayFirst(self.chatRooms(), function (item) {
                return updatedRoom.id() == item.id();
            });

            room.name(updatedRoom.name());

            if (self.joinedRoomId() == room.id()) {
                self.joinRoom(room);
            }
        }

        self.roomDeleted = function (id) {
            var temp;
            ko.utils.arrayForEach(self.chatRooms(), function (room) {
                if (room.id() == id)
                    temp = room;
            });
            self.chatRooms.remove(temp);
        }

        self.messageDeleted = function (id) {
            var temp;
            ko.utils.arrayForEach(self.chatMessages(), function (message) {
                if (message.id() == id)
                    temp = message;
            });
            self.chatMessages.remove(temp);
        }

        self.userAdded = function (user) {
            var temp;
            ko.utils.arrayForEach(self.chatUsers(), function (u) {
                if (u.connectionId() == user.connectionId()) temp = u;
            });
            if (temp) return;
            self.chatUsers.push(user);
        }

        self.userRemoved = function (id) {
            var temp;
            ko.utils.arrayForEach(self.chatUsers(), function (user) {
                if (user.connectionId() == id)
                    temp = user;
            });
            self.chatUsers.remove(temp);
        }

        self.uploadFiles = function () {
            var form = document.getElementById("uploadForm");
            $.ajax({
                type: "POST",
                data: new FormData(form),
                contentType: false,
                processData: false,
                success: function () {
                    $("#UploadedFile").val("");
                },
                error: function (error) {
                    alert('Error: ' + error.responseText);
                }
            });
        }
    }

    function ScrollToEnd(container, lastScrollTop) {
        var c = $(container);
        const cc = c[0];
                
        c.animate({ scrollTop: cc.scrollHeight },
        {
            duration: 800,
            complete: function () {
                if (lastScrollTop == cc.scrollTop) return;
                ScrollToEnd(container, cc.scrollTop);             
            }
        });

    }

    function ChatRoom(id, name) {
        var self = this;
        self.id = ko.observable(id);
        self.name = ko.observable(name);
    }

    function ChatUser(userName, displayName, avatar, currentRoom, device,connectionId) {
        var self = this;
        self.userName = ko.observable(userName);
        self.displayName = ko.observable(displayName);
        self.avatar = ko.observable(avatar);
        self.currentRoom = ko.observable(currentRoom);
        self.device = ko.observable(device);
        self.connectionId = ko.observable(connectionId);
    }

    function ChatMessage2(messageView, isMine, isSystemMessage) {  
        const mv = messageView;
        var msg = new ChatMessage(mv.id, mv.content, mv.timestamp, mv.from, isMine, mv.avatar);

        if (isSystemMessage) msg.isSystemMessage( isSystemMessage);
        return msg;
    }

    function ChatMessage(id, content, timestamp, from, isMine, avatar) {
        var self = this;
        self.isSystemMessage = ko.observable(false);
        self.id = ko.observable(id);

        self.content = ko.observable(content);
        self.timestamp = ko.observable(timestamp);
        self.timestampRelative = ko.pureComputed(function () {
            // Get diff
            var date = new Date(self.timestamp());
            var now = new Date();
            var diff = Math.round((date.getTime() - now.getTime()) / (1000 * 3600 * 24));

            // Format date
            var { dateOnly, timeOnly } = formatDate(date);

            // Generate relative datetime
            if (diff == 0)
                return `Today, ${timeOnly}`;
            if (diff == -1)
                return `Yestrday, ${timeOnly}`;

            return `${dateOnly}`;
        });
        self.timestampFull = ko.pureComputed(function () {
            var date = new Date(self.timestamp());
            var { fullDateTime } = formatDate(date);
            return fullDateTime;
        });
        self.from = ko.observable(from);
        self.isMine = ko.observable(isMine);
        self.avatar = ko.observable(avatar);
    }

    function formatDate(date) {
        // Get fields
        var year = date.getFullYear();
        var month = date.getMonth() + 1;
        var day = date.getDate();
        var hours = date.getHours();
        var minutes = date.getMinutes();
        var d = hours >= 12 ? "PM" : "AM";

        // Correction
        if (hours > 12)
            hours = hours % 12;

        if (minutes < 10)
            minutes = "0" + minutes;

        // Result
        var dateOnly = `${day}/${month}/${year}`;
        var timeOnly = `${hours}:${minutes} ${d}`;
        var fullDateTime = `${dateOnly} ${timeOnly}`;

        return { dateOnly, timeOnly, fullDateTime };
    }

    var viewModel = new AppViewModel();
    ko.applyBindings(viewModel);

    viewModel.na1ta(na1ta);
    viewModel.caseId(caseId);
    viewModel.adminId($('#inputAdminId').val());

});
