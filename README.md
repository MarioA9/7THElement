## Veronica la heroina musical y el Septimo Elemento

Aqui detallare todo lo que se pudo avanzar en el juego a lo largo de estos dias:

### Personaje

Con el personaje logramos implementar la mayoria de sus mecanicas, por ejemplo su movimiento, sus ataques, interaccion con objetos NPC y otras cosas, 
para poder ejecutar el ataque elemental y la Ultimate necesitas conseguir elementos, por el momento solo se puede conseguir el elemento de Voz, pero 
para que se pueda probar puede asignarlos momentaneamente en el inspector del Personaje, se necesitan dos elementos para usar la elemental y cinco 
elementos para usar la Ultimate, tambien hay algo caracteristico, si no consigues el elemento de Voz, los NPC no podran comunicarse contigo lo que 
hara mas complicado la aventura y no podras conseguir pistas.

### Enemigos

Los enemigos por el momento solo hay un enemigo normal establecido en el mapa para probar, pero ya se pueden asignar diferentes enemigos, como enemigo
pesado y jefe, que tendran ataques y daños diferentes, los enemigos tienen un area para explorar hasta que se acerque el player y asi puedan atacarlo.

### NPC

Actualmente no hay NPC establecidos en el mapa pero su codigo ya esta listo y ya funciona correctamente, podran comunicarse con el Player asignandole
tanto el codigo los NPC y el de los dialogos, en caso de que el Player no tenga el elemento Voz, la conversacion puede cambiar completamente, ya que 
el Player no podra comunicarse al no tener voz

### Mapas 

Actualmente ya estan terminado el Mapa de inicio que es una zona de pradera que puede llevarte a varias zonas, tambien ya esta listo tanto el bioma de 
nieves, el desierto y el bosque magico donde el Player conseguira el elemento de Voz y podra conseguir el septimo elemento al final del juego(todavia 
no asignado), hay un mapa de prueba que es de una zona volcanica pero no sera el definitivo

### Dialogos

El sistema de dialogo es uno de los apartados mas completos dentro del juego, tiene tantas funciones distintas para adaptarse a la situacion del Player
como interactuar con un NPC o interactuar con un objeto, o elemento, tambien al sistema de dialogo le importara si el Player cuenta con el elemento de
Voz, en caso de no tenerlo, sucedera una conversacion completamente distinta en el que el Player solo tendra pensamientos y no podra hablar, en caso de
interactuar con un NPC, el NPC no podra comunicarse contigo y la conversacion terminara rapido, tambien hay una funcion de recuerdo, cuando el Player
pueda hablar, podra recordar con que NPC se comunico y tendra un dialogo distinto en el que recordara que ya hablo con ese NPC.

### Sistema de guardado

Actualmente el sistema de guardado sigue en un punto en el que aun tiene mucho margen de mejora, el sistema de guardado funciona con la posicion del jugador
y los elementos que ya cuenta, en estos momentos se esta trabajando para que tambien guarde elementos eliminados, al principio medio funcionaba eso pero al
tratar de mejorarlo decayo un poco ahora sin funcionar el guardado de objetos eliminados(por ejemplo, cuando consigues un elemento o entras en una zona de
dialogo como al incio del juego)

### Conclusion

El juego todavia tiene mucho margen para mejorar pero con el tiempo que he tenido para hacer esto, otros proyectos y mi participacion en la Game Jam, siento
que voy bien al ritmo que tomamos, esperemos que en vacaciones tenga mucho mas tiempo a dedicar este proyecto
