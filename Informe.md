# Informe de funcionamiento

- En el **Network Manager UI** se gestiona la selección de conexión (host, server, client).
  
- En **Game Manager** si se es Servidor, se gestiona mediante una corrutina la elección de manera aleatoria de cuál de los jugadores va a tener la ventaja o desventaja. También se le asigna de manera aleatoria un estado (ventaja o desventaja), y finalmente se inicia la corrutina de "limpieza" de los efectos del estado, es decir, se vuelve a poner el estado del Player en normal.
  
- **Player Controller** gestiona el spawneo aleatorio en el plano y movimiento del player de la misma manera que en el ejercicio anterior, mediante un `Rpc(SendTo.Server)`. En el que ahora he añadido un switch sobre una Network Variable (estado actual: ventaja, desventaja, normal), que compara la variable asignada mediante `SetState` desde el **Game Manager** con los valores del enumerado, para dar una velocidad diferente en función de su valor.  
Para el cambio del color nos suscribimos a un evento que se lanza cuando cambia el valor la Network Variable.