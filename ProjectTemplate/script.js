var contentPanels = ['logonPanel', 'newAccount', 'suggestionPage', 'accountsPanel', 'leaderBoardPage', 'matchingPage', 'login-container'];
//this function toggles which panel is showing, and also clears data from all panels
function showPanel(panelId) {

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

                showPanel('accountsPanel');   //When user logins with correct credentials it will take them to main content page
            } else {
                alert("Bad credentials");

            }
        },
        error: function (e) {
            alert("this code will only execute if javascript is unable to access the webservice");
            showPanel('accountsPanel'); //CHANGE WHEN DB IS UP
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
            showPanel("newAccount");
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

function suggestionInput() {
    window.location.href = "feedback.html";
    //showPanel('suggestionPage');
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

document.getElementById('logonID').addEventListener('keyup', handleEnterKey);
document.getElementById('logonPass').addEventListener('keyup', handleEnterKey);

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
        })
        .then(text => parseXMLResponse(text));
}

// Function to populate dropdown with options
function populateDropdown(elementId, data) {
    var dropdown = document.getElementById(elementId);
    dropdown.innerHTML = "";
    // Loop through the data array and create an option element for each item
    data.forEach(function (item) {
        var option = document.createElement("option");
        option.value = item;
        option.textContent = item;
        dropdown.appendChild(option);
    });
}

// Parse XML response
function parseXMLResponse(xmlString) {
    // Create a parser object
    const parser = new DOMParser();
    // Parse XML into a document
    const xmlDoc = parser.parseFromString(xmlString, "text/xml");
    // Get all "string" elements
    const stringElements = xmlDoc.getElementsByTagName("string");
    // Array to store topics
    const topics = [];
    // Extract text content from each string element
    for (let i = 0; i < stringElements.length; i++) {
        topics.push(stringElements[i].textContent);
    }
    // Return topics array
    return topics;
}

// Fetch topics and populate the topics dropdown
fetchData("ProjectServices.asmx/GetTopics")
    .then(function (topics) {
        populateDropdown("topicSelect", topics);
    })
    .catch(function (error) {
        console.error('Error fetching topics:', error);
    });

// Get the topic from the dropdown and fetch questions based on topic. 
// Changing this to use topic id instead of topic name would be far more elegant. See GetQuestions service. 
document.getElementById("topicSelect").addEventListener("change", function () {
    var selectedTopic = this.value;
    fetchData("ProjectServices.asmx/GetQuestions?topicName=" + selectedTopic)
        .then(function (questions) {
            populateDropdown("questionSelect", questions);
        })
        .catch(function (error) {
            // Log an error message if fetching questions fails
            console.error('Error fetching questions:', error);
        });
});
//end of feedback js code

// Matching page to allow swipes of agree or skip
