
var contentPanels = ['logonPanel', 'newAccount', 'suggestionPage', 'accountsPanel', 'leaderBoardPage', 'matchingPage', 'login-container', 'manager'];
let pointTotal = 0;
let startPoint;
let offsetX;
let offsetY;
let isDragging = false;
let isManager = false;
let logoPic;



// initialzing the Swipe function for matching page
const init = () => {


    logoPic = document.getElementById('logoPic');// Need to change to show Topics, Questions and Suggestions


    const handleMouseMove = (e) => {
        e.preventDefault();
        if (!startPoint) return;
        const { clientX, clientY } = e;
        offsetX = clientX - startPoint.x;
        offsetY = clientY - startPoint.y;
        logoPic.style.transform = `translate(${offsetX}px, ${offsetY}px)`;
    };

    logoPic.addEventListener('dragstart', e => {
        e.preventDefault();
    })

    const handleMouseUp = () => {
        if (!isDragging) return;
        isDragging = false;
        logoPic.style.transition = 'transform 0.5s ease';

        // able to grab center of image/suggestion when updated
        if (Math.abs(offsetX) > logoPic.offsetWidth / 4) {
            handleSwipe(offsetX > 0);

        } else {
            logoPic.style.transform = 'translateX(0)';
        }

        setTimeout(() => {
            logoPic.style.transition = '';
            logoPic.style.opacity = 1; // picture disappears when pulled left or right and new suggestion shows
        }, 1000);
    };

    const listenToMouseEvents = () => {
        logoPic.addEventListener('mousedown', (e) => {
            const { clientX, clientY } = e;
            startPoint = { x: clientX, y: clientY };
            document.addEventListener('mousemove', handleMouseMove);
            isDragging = true;
            logoPic.style.transition = 'transform 0s';
        });

        document.addEventListener('mouseup', () => {
            if (isDragging) {
                document.removeEventListener('mousemove', handleMouseMove);
                handleMouseUp();
            }
        });

    };


    listenToMouseEvents();
};

function handleSwipe(liked) {
    if (liked) {
        console.log("Liked!");
        animateSwipe('right');

    } else {
        console.log("Disliked!");
        animateSwipe('left');
    }
}


// animation for when button agree or skip is clicked to move Suggestions
function animateSwipe(direction) {
    logoPic.style.transition = 'transform 1s';
    logoPic.style.transform = `translate(${direction * window.innerWidth}px, ${offsetY}px) rotate(${90 * direction}deg)`;
    logoPic.style.opacity = 0;
    logoPic.classList.add('dismissing');
    setTimeout(() => {

        showNextCard();
    }, 1000);
}

function resetLogoPic() {

    logoPic.style.transform = 'translateX(0)';
    logoPic.style.opacity = 1;
}

//function to move onto next auggestion Topic
function showNextCard() {

    console.log("Showing the next card...");
    resetLogoPic(); // Reset the logoPic position for the next card
    logoPic.classList.remove('dismissing');
}

// init();


//this function toggles which panel is showing, and also clears data from all panels
function showPanel(panelId) {
    clearLogon();
    for (var i = 0; i < contentPanels.length; i++) {
        if (panelId == contentPanels[i]) {
            $("#" + contentPanels[i]).css("visibility", "visible");
        }
        else {
            $("#" + contentPanels[i]).css("visibility", "hidden");
        }
    }

   
}



jQuery(function () {
    //when the app loads, show the logon panel to start with!
    showPanel('logonPanel');
});


function TestButtonHandler() {
    var userCountValue = document.getElementById("userCountInput").value;
    var webMethod = "ProjectServices.asmx/getBries";
    var parameters = "{\"userCount\":\"" + encodeURI(userCountValue) + "\"}";

    $.ajax({
        type: "POST",
        url: webMethod,
        data: parameters,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            var responseFromServer = msg.d;
            for (var i = 0; i < responseFromServer.length; i++) {
                alert(responseFromServer[i].firstName);
                alert(responseFromServer[i].lastName);
                alert(responseFromServer[i].phoneNumber);
            }
            doThisAfterServerStuff();
        },
        error: function (e) {
            alert("this code will only execute if JavaScript is unable to access the web service");
        }
    });

    function doThisAfterServerStuff() {
        alert("you thought this would execute after the code in my success function, didn't you!?");
    }
}


function logon() {
    var id = document.getElementById("logonID").value;
    var pass = document.getElementById("logonPass").value;
    var webMethod = "ProjectServices.asmx/LogOn";
    var parameters = "{\"uid\":\"" + encodeURI(id) + "\", \"pass\":\"" + encodeURI(pass) + "\"}";

    $.ajax({
        type: "POST",
        url: webMethod,
        data: parameters,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            var responseFromServer = msg.d;
            if (responseFromServer == true) {

                var isManager = responseFromServer.isManager;

                // Show the main content panel
                showPanel('accountsPanel');

                // Update the visibility of the manager-specific link
                if (id === "manager") {
                    document.getElementById('managerView').style.display = 'block';
                } else {
                    document.getElementById('managerView').style.display = 'none';
                }

            }
        },
        error: function (e) {
            alert("this code will only execute if javascript is unable to access the webservice");
            showPanel('accountsPanel'); //CHANGE WHEN DB IS UP
            document.getElementById("usernameShow").innerHTML = id;
        }
    });
}

function CreateAccount() {
    var id = document.getElementById("newId").value;
    var pass = document.getElementById("newPassword").value;
    var webMethod = "ProjectServices.asmx/RequestAccount";
    var parameters = "{\"uid\":\"" + encodeURI(id) + "\", \"pass\":\"" + encodeURI(pass) + "\"}";

    $.ajax({
        type: "POST",
        url: webMethod,
        data: parameters,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            showPanel("newAccount"); //shows page to create account
            alert("Welcome to Work Pulse!");
            showPanel('logonPanel');
        },
        error: function (e) {
            alert("Something went wrong");
        }
    });
}

//each button selected will take them to designated page
function leaderBoard() {
    showPanel('leaderBoardPage');
}

function match() {
    showPanel('matchingPage');

}

function viewTickets() {
    showPanel('manager');
    
}

function suggestionInput() {
    window.location.href = "feedback.html";
    //showPanel('suggestionPage');
}

function pointCounter(userid) {

    $.ajax({
        url: "ProjectServices.asmx/GetLikes",
        type: "GET",
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        data: JSON.stringify({ userid: userid }),
        success: function (response) {
            var points = parseInt(response.d);
            document.getElementById('numberCounter').innerHTML = points;
        },
        error: function (xhr, status, error) {
            console.error("Error fetching points:", error);
        }
    });
    /*pointTotal++;

    document.getElementById('numberCounter').innerHTML = pointTotal;
    
    console.log(pointTotal);*/
}

// Allows the user to go back to main content page
function goBack() {
    showPanel('accountsPanel');
}

// Call logon function when Enter key is pressed
function handleEnterKey(event) {
    if (event.keyCode === 13) { //Enter key
        logon(); // Call the logon function
    }
}

//Clears logon data 
function clearLogon() {
    $('#logonID').val("");
    $('#logonPass').val("");
}

function LogOff() {

    var webMethod = "ProjectServices.asmx/LogOff";
    $.ajax({
        type: "POST",
        url: webMethod,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            if (msg.d) {
                //we logged off, so go back to logon page,
                //stop checking messages
                //and clear the chat panel
                showPanel('logonPanel');
                
            }
            else {
            }
        },
        error: function (e) {
            alert("Error");
        }
    });
}


/* test function for incrementSwipes follows
function incrementSwipes() {
    var userid = "Steven"; // Hardcoded for testing, can make dynamic when implemented
    var webMethod = "ProjectServices.asmx/IncrementSwipes";
    var parameters = "{\"userid\":\"" + encodeURI(userid) + "\"}";

    $.ajax({
        type: "POST",
        url: webMethod,
        data: parameters,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            alert("Swipes incremented successfully.");
        },
        error: function (e) {
            alert("Error: Unable to increment swipes.");
        }
    });
}*/

//feedback js code
function fetchData(url) {
    return fetch(url)
        .then(response => {
            // Check if the response is successful (status code in the range 200-299)
            // If response is not ok, throw an error
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.text();
        });
}

// Function to populate dropdown with options
function populateDropdown(elementId, data) {
    var dropdown = document.getElementById(elementId);
    dropdown.innerHTML = "";
    // Loop through the data array and create an option element for each item
    data.forEach(function (item) {
        var option = document.createElement("option");
        option.value = item.id; // Use the item's ID as the option value
        option.textContent = item.name; // Use the item's name as the option text
        dropdown.appendChild(option);
    });
}

// Parse XML response
function parseXMLResponse(xmlString, type) {
    const parser = new DOMParser();
    const xmlDoc = parser.parseFromString(xmlString, "application/xml");
    let items = [];
    let elements = xmlDoc.getElementsByTagName(type);

    for (let i = 0; i < elements.length; i++) {
        const id = elements[i].getElementsByTagName("Id")[0].textContent;
        const text = elements[i].getElementsByTagName(type === "Topic" ? "Name" : "Text")[0].textContent;
        items.push({ id: id, name: text });
    }
    console.log(items);

    return items;
}




// Suggestion submission code. The wrappers are needed to prevent the script from loading except on
// feedback.html
document.addEventListener('DOMContentLoaded', function () {
    var submitButton = document.getElementById('submitFeedback');
    if (submitButton) {
        submitButton.addEventListener('click', function (event) {
            event.preventDefault(); 

            // Capture user inputs
            var userId = "5"; // This needs to be dynamically set based on the logged-in user
            var topicId = document.getElementById("topicSelect").value;
            var questionId = document.getElementById("questionSelect").value;
            var suggestionText = document.getElementById("feedbackValue").value;

            // Construct the parameters string for the AJAX call
            var parameters = JSON.stringify({
                uid: userId,
                questionId: questionId,
                topicId: topicId,
                suggestionText: suggestionText
            });

            // Make the AJAX call to your web service
            $.ajax({
                type: "POST",
                url: "ProjectServices.asmx/SubmitSuggestion",
                data: parameters,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (response) {
                    alert("Suggestion submitted successfully!");
                },
                error: function (error) {
                    console.error("Error submitting suggestion:", error);
                    alert("Error submitting suggestion.");
                }
            });
        });     //JavaScript isn't fun :(
    }
});


//end of feedback js code



function ticketManager() {
    var id = document.getElementById('userid').value;
    var webMethod = "ProjectServices.asmx/TotalTicketCount";
    var parameters = "{\"uid\":\"" + encodeURI(id) + "\"}";


    $.ajax({
        type: "POST",
        url: webMethod,
        data: parameters,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            console.log("pass");
            var tickets = msg.d

            var displayTicketsContainer = document.getElementById("displayTickets");
            displayTicketsContainer.innerHTML = id + " has " + tickets + " tickets";

        },
        error: function (e) {
            alert("Error fetching tickets");
        }
    });
}





