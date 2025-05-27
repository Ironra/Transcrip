// Obtiene los elementos de la interfaz
const startBtn = document.getElementById('start');
const stopBtn  = document.getElementById('stop');
const logEl    = document.getElementById('log');

// Variable donde se acumula el texto final reconocido
let currentText = '';

// Declaración de variables globales
let ws, audioCtx, processor, input;

// Función para abrir el WebSocket con el backend
function openSocket() {
  // Determina si usar "ws://" o "wss://" según el protocolo del sitio
  const protocol = location.protocol === 'https:' ? 'wss' : 'ws';

  // Construye la URL del WebSocket usando el host actual
  const socketUrl = protocol + '://' + location.host + '/ws';

  // Abre el WebSocket
  ws = new WebSocket(socketUrl);

  // Evento que se dispara al recibir mensajes del servidor
  ws.onmessage = e => {
    console.log('WebSocket abierto');
    
    // Convierte el mensaje recibido en objeto JSON
    const msg = JSON.parse(e.data);

    // Si es un resultado parcial, lo muestra sin acumular
    if (msg.partial) {
      logEl.textContent = `${currentText}${msg.partial}`;
    
    // Si es un resultado final, lo acumula
    } else if (msg.text) {
      currentText += msg.text + ' ';
      logEl.textContent = currentText;
    }
  };
}

// Evento que se dispara al hacer clic en "Start"
startBtn.onclick = async () => {
  openSocket();  // Abre el WebSocket

  // 1) Crea un AudioContext con una frecuencia de muestreo de 16000 Hz (compatible con Vosk)
  audioCtx = new (window.AudioContext || window.webkitAudioContext)({ sampleRate: 16000 });

  // Solicita acceso al micrófono
  const stream = await navigator.mediaDevices.getUserMedia({ audio: true });

  // Crea una fuente de audio a partir del stream
  input = audioCtx.createMediaStreamSource(stream);

  // 2) Crea un ScriptProcessorNode para procesar el audio en chunks
  processor = audioCtx.createScriptProcessor(4096, 1, 1);  // Buffer de 4096 muestras, mono

  // Evento que se dispara cada vez que hay audio disponible
  processor.onaudioprocess = e => {
    console.log('onaudioprocess fired');

    // Obtiene los datos en Float32 [-1..1]
    const floatData = e.inputBuffer.getChannelData(0);

    // Convierte Float32 a Int16 (formato requerido por Vosk)
    const int16 = new Int16Array(floatData.length);
    for (let i = 0; i < floatData.length; i++) {
      let s = Math.max(-1, Math.min(1, floatData[i]));
      int16[i] = s < 0 ? s * 0x8000 : s * 0x7FFF;
    }

    // Envía el buffer de audio al servidor si el WebSocket está abierto
    if (ws.readyState === WebSocket.OPEN) {
      console.log('Enviando audio:', int16.byteLength, 'bytes');
      ws.send(int16.buffer);
    }
  };

  // 3) Conecta los nodos de audio
  input.connect(processor);
  processor.connect(audioCtx.destination); // Aunque no necesitas oírlo, esto es requerido por algunos navegadores

  // Actualiza los estados de los botones
  startBtn.disabled = true;
  stopBtn.disabled  = false;
};

// Evento que se dispara al hacer clic en "Stop"
stopBtn.onclick = () => {
  // 1) Detiene y desconecta el procesamiento de audio
  processor.disconnect();
  input.disconnect();
  audioCtx.close();

  // 2) Cierra la conexión WebSocket (el servidor enviará el resultado final)
  ws.close();

  // Actualiza los estados de los botones
  startBtn.disabled = false;
  stopBtn.disabled  = true;
};
