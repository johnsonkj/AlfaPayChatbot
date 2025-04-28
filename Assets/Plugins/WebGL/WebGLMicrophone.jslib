mergeInto(LibraryManager.library, {
  InitMicrophone: function() {
    if (!window.unityMicrophoneSystem) {
      window.unityMicrophoneSystem = {
        audioContext: null,
        mediaStream: null,
        recorder: null,
        audioData: [],
        isRecording: false,
        
        init: function() {
          try {
            this.audioContext = new (window.AudioContext || window.webkitAudioContext)({sampleRate: 16000});
            console.log("WebGL Microphone system initialized");
            return true;
          } catch (e) {
            console.error("Failed to initialize WebGL Microphone system:", e);
            return false;
          }
        },
        
        startRecording: function() {
          const self = this;
          
          if (this.isRecording) {
            console.warn("Already recording");
            return false;
          }
          
          if (!this.audioContext) {
            if (!this.init()) return false;
          }
          
          return navigator.mediaDevices.getUserMedia({ audio: true, video: false })
            .then(function(stream) {
              self.mediaStream = stream;
              const source = self.audioContext.createMediaStreamSource(stream);
              self.recorder = self.audioContext.createScriptProcessor(4096, 1, 1);
              
              self.audioData = [];
              self.recorder.onaudioprocess = function(e) {
                const audioBuffer = e.inputBuffer.getChannelData(0);
                const dataArray = new Float32Array(audioBuffer);
                self.audioData.push(dataArray);
              };
              
              source.connect(self.recorder);
              self.recorder.connect(self.audioContext.destination);
              self.isRecording = true;
              console.log("WebGL Microphone recording started");
              return true;
            })
            .catch(function(err) {
              console.error("Failed to start WebGL Microphone recording:", err);
              return false;
            });
        },
        
        stopRecording: function() {
          if (!this.isRecording) {
            console.warn("Not recording");
            return null;
          }
          
          this.recorder.disconnect();
          
          if (this.mediaStream) {
            this.mediaStream.getTracks().forEach(function(track) {
              track.stop();
            });
          }
          
          // Combine all audioData arrays into one
          const totalLength = this.audioData.reduce((acc, val) => acc + val.length, 0);
          const combinedData = new Float32Array(totalLength);
          
          let offset = 0;
          for (let i = 0; i < this.audioData.length; i++) {
            combinedData.set(this.audioData[i], offset);
            offset += this.audioData[i].length;
          }
          
          this.isRecording = false;
          console.log("WebGL Microphone recording stopped");
          
          // Convert to WAV
          const wav = this.encodeWAV(combinedData);
          const base64 = this.arrayBufferToBase64(wav);
          return base64;
        },
        
        encodeWAV: function(samples) {
          const buffer = new ArrayBuffer(44 + samples.length * 2);
          const view = new DataView(buffer);
          
          // RIFF identifier
          this.writeString(view, 0, 'RIFF');
          // RIFF chunk length
          view.setUint32(4, 36 + samples.length * 2, true);
          // RIFF type
          this.writeString(view, 8, 'WAVE');
          // format chunk identifier
          this.writeString(view, 12, 'fmt ');
          // format chunk length
          view.setUint32(16, 16, true);
          // sample format (1 is PCM)
          view.setUint16(20, 1, true);
          // channel count
          view.setUint16(22, 1, true);
          // sample rate
          view.setUint32(24, 16000, true);
          // byte rate (sample rate * block align)
          view.setUint32(28, 16000 * 2, true);
          // block align (channel count * bytes per sample)
          view.setUint16(32, 2, true);
          // bits per sample
          view.setUint16(34, 16, true);
          // data chunk identifier
          this.writeString(view, 36, 'data');
          // data chunk length
          view.setUint32(40, samples.length * 2, true);
          
          // Write the PCM samples
          const volume = 1;
          let index = 44;
          for (let i = 0; i < samples.length; i++) {
            view.setInt16(index, samples[i] * 0x7FFF * volume, true);
            index += 2;
          }
          
          return buffer;
        },
        
        writeString: function(view, offset, string) {
          for (let i = 0; i < string.length; i++) {
            view.setUint8(offset + i, string.charCodeAt(i));
          }
        },
        
        arrayBufferToBase64: function(buffer) {
          const bytes = new Uint8Array(buffer);
          let binary = '';
          for (let i = 0; i < bytes.byteLength; i++) {
            binary += String.fromCharCode(bytes[i]);
          }
          return window.btoa(binary);
        }
      };
    }
    
    return window.unityMicrophoneSystem.init();
  },
  
  RequestMicrophonePermission: function() {
    if (!window.unityMicrophoneSystem) {
      // Use the Unity function caller method instead of UnityInstance
      dynCall_v(InitMicrophone);
    }
    
    navigator.mediaDevices.getUserMedia({ audio: true, video: false })
      .then(function(stream) {
        // Stop the stream immediately, we just wanted permission
        stream.getTracks().forEach(function(track) {
          track.stop();
        });
        
        // Use gameObject-method pattern for sending messages back to Unity
        var gameObject = UTF8ToString('WebGLMicrophone');
        var functionName = 'OnMicrophonePermissionGranted';
        SendMessage(gameObject, functionName);
      })
      .catch(function(err) {
        console.error("Microphone permission denied:", err);
        
        var gameObject = UTF8ToString('WebGLMicrophone');
        var functionName = 'OnMicrophonePermissionDenied';
        SendMessage(gameObject, functionName);
      });
    
    return true;
  },
  
  StartRecording: function() {
    if (!window.unityMicrophoneSystem) {
      // Use the Unity function caller method instead of UnityInstance
      dynCall_v(InitMicrophone);
    }
    
    window.unityMicrophoneSystem.startRecording()
      .then(function(success) {
        if (success) {
          SendMessage('WebGLMicrophone', 'OnRecordingStarted');
        } else {
          SendMessage('WebGLMicrophone', 'OnRecordingError');
        }
      });
    
    return true;
  },
  
  StopRecording: function() {
    if (!window.unityMicrophoneSystem || !window.unityMicrophoneSystem.isRecording) {
      return "";
    }
    
    const base64Data = window.unityMicrophoneSystem.stopRecording();
    SendMessage('WebGLMicrophone', 'OnRecordingComplete', base64Data);
    return allocate(intArrayFromString(base64Data), ALLOC_NORMAL);
  },
  
  IsRecording: function() {
    if (!window.unityMicrophoneSystem) return false;
    return window.unityMicrophoneSystem.isRecording;
  }
});