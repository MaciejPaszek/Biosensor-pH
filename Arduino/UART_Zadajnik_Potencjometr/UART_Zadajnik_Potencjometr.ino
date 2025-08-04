void setup() {

  // Wbudowana dioda jako wyjście
  pinMode(13, OUTPUT);
  pinMode(12, OUTPUT);

  pinMode(A0, INPUT);
  pinMode(A1, INPUT);
  pinMode(A2, INPUT);

  digitalWrite(13, LOW);
  digitalWrite(12, HIGH);

  // Inicjalizacja transmisji szeregowej
  Serial.begin(9600);

  while (!Serial);
}

bool waitForStart = true;

void loop() {

  while(waitForStart) {

    if (Serial.available() > 0) {

      String command = Serial.readString();

      command.toUpperCase();

      if(command.startsWith("START"))
      {
        waitForStart = false;
        digitalWrite(13, HIGH);
        digitalWrite(12, LOW);
      }
      else
        Serial.println("Command not supported. Waiting for START command.");

      if(command.startsWith("END"))
      {
        Serial.println("Waiting for START command.");
      }
    }

    delay(1);
  };

  float temperaturaProbki = (analogRead(A0) * 40.0) / 1024.0;
  float temperaturaOtoczenia = (analogRead(A1) * 40.0) / 1024.0;
  float wilgotnoscOtoczenia = (analogRead(A2) * 100.0) / 1024.0;

  Serial.print(temperaturaProbki, 1);
  Serial.print(" ");
  Serial.print(temperaturaOtoczenia, 1);
  Serial.print(" ");
  Serial.print(wilgotnoscOtoczenia, 0);
  Serial.println();

  if (Serial.available() > 0) {

    String command = Serial.readString();

    command.toUpperCase();

    if(command.startsWith("END"))
    {
      waitForStart = true;
      digitalWrite(13, LOW);
      digitalWrite(12, HIGH);
    }
  }

  delay(100);
}






