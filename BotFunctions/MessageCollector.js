//This Module collects all message that need to be  sent to the player and agregates them into a single message. When the Return function is called, they are returned back for sending.

// -----------------------------------------------------------------------------------------------------------------------------------------------------------

var messageSoFar = '';
var attach = false;
var attachPath = '';
var attachName = '';
var Prom = [];

module.exports = {

    Add(textToAdd){
        messageAdder(textToAdd);
    },
    async AddFile(path,fileName){
        attach = true;
        attachPath = await path;
        attachName = await fileName;
    },
    Return(){
        if(messageSoFar.length > 1960){
            return `Message is too long for one message: \n${messageSoFar.substring(0,1960)}`;
        }
        return messageSoFar;
    },
    Clear(){
        messageSoFar = '';
        attach = false;

    },
    hasAttach(){
        return attach;
    },
    ReturnAttachData(){
        return [attachPath,attachName];
    },
    setWait(promise){
        Prom.push(promise);
    },
    isWaiting(){
        return Prom;
    }
}

async function messageAdder(textToAdd){
    var actualText = await textToAdd; // could be promise so just to make sure....
    if(actualText){ //check for content (due to JS, if there is ANYTHING in the string, it will return true since the binary value is more than 1.)
        return messageSoFar = `${messageSoFar}${actualText}\n`;
    }
    else{
        console.log('Empty message was submitted to the Message collector.');
    }
}