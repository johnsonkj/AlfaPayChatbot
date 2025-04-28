mergeInto(LibraryManager.library, {
  PlayAudioFromBase64JS: function(base64DataPtr) {
    var base64Data = UTF8ToString(base64DataPtr);
    
    // Create audio element
    var audio = new Audio("data:audio/mp3;base64," + base64Data);
    
    // Play the audio
    var playPromise = audio.play();
    
    if (playPromise !== undefined) {
      playPromise.then(function() {
        // Audio is playing
        console.log("Audio is playing");
      }).catch(function(error) {
        // Auto-play was prevented
        console.error("Playback failed:", error);
      });
    }
  }
});