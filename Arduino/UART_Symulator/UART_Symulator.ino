#include "Inercja.h"

double TempMax  = 25;
double TempMin  = 8;
double Temp0    = 20;
double T1       = 50;
double T2       = 10;

double u = 1;
double h = 0.1;

INERCJA Inercja1;
INERCJA Inercja2;

// Zmienna przechowująca wprowadzoną komendę
String command = "";

void setup() {

  // Wbudowana dioda jako wyjście
  pinMode(13, OUTPUT);
  pinMode(12, OUTPUT);

  pinMode(A0, INPUT);
  pinMode(A1, INPUT);
  pinMode(A2, INPUT);

  digitalWrite(13, LOW);
  digitalWrite(12, HIGH);

  // Inercje
  Inercja1 = INERCJA(TempMax - TempMin, 10, Temp0 - TempMin);
  Inercja2 = INERCJA(1,  10, Temp0 - TempMin);

  // Inicjalizacja transmisji szeregowej
  Serial.begin(9600);

  while (!Serial);
}

bool transmit = false;

// Pętla wykonywana co 1 ms
void loop() {

  // Odczytanie czasu
  unsigned long currentTime = millis();
  
  // Wykonanie funkcji w cyklu
  Looper(currentTime);

  // Odbieranie przychodzących znaków z portu szeregowego
  if(Serial.available() > 0) {

    // Odczytaj pojedynczy znak
    char c = Serial.read();

    // Znak końca linii oznacza koniec komendy
    if(c == '\n') {
      
      // Formalizacja komend
      command.toUpperCase();

      // Interpretacja komendy - wykonanie odpowiednich metod
      if(!InterpretCommand(command))
        Serial.println("Invaild command: " + command);
      
      // Czyszczenie bufora komendy
      command = "";
    }
    else
      // Znaki niebędące znakiem końca linii są dopisywane do bufora
      command += c;
  }

  // Opóżnienie 1 ms
  delay(1);
}

unsigned long lastTransmit = 0;
unsigned long transmitInterval = 100;

void Looper(unsigned long currentTime) {

  if(currentTime - lastTransmit >= transmitInterval)
  {
    // Symulacja Inercji II-go rzędu 
    double x = Inercja1.Euler(u, h);
    double y = Inercja2.Euler(x, h);

    double temperaturaProbki = y + TempMin;
    double temperaturaOtoczenia = TempMax;
    double wilgotnoscOtoczenia = 50;

    if(transmit)
    {
      Serial.print(temperaturaProbki, 1);
      Serial.print(" ");
      Serial.print(temperaturaOtoczenia, 1);
      Serial.print(" ");
      Serial.print(wilgotnoscOtoczenia, 0);
      Serial.println();
    }

    lastTransmit = currentTime;
  }

}

// Interpretacja komend
bool InterpretCommand(String command)
{
  // Łańcuch znaków zawierających argumenty komendy (bez nazwy komendy)
  String argumentsString = "";

  // Pozycja pierwszej spacji
  int index = command.indexOf(" ");
  
  // Wydzielenie argumentów (Za pierwszą spacją)
  if(index != -1)
    argumentsString = command.substring(index + 1);
  
  if(command.startsWith("START")) {
    //Serial.println("Arduino is now in START mode.");
    transmit = true;
    return true;
  }

  if(command.startsWith("STOP")) {
    //Serial.println("Arduino is now in STOP mode.");
    transmit = false;
    return true;
  }

  if(command.startsWith("ON")) {
    u = 0;
    digitalWrite(13, HIGH);
    //Serial.println("Cooling is now turned ON.");
    return true;
  }

  if(command.startsWith("OFF")) {
    u = 1;
    digitalWrite(13, LOW);
    //Serial.println("Cooling is now turned OFF.");
    return true;
  }

  // Przykłąd komendy wieloparametrowej
  /*
  if(command.startsWith("SV")) {
    const int minArguments = 1;
    const int maxArguments = 2;

    String arguments[maxArguments] = {""};

    int length = Arguments(argumentsString, arguments, maxArguments);

    if(length < minArguments)
      return false;
    
    if(length > maxArguments)
      return false;

    if(length == 1)
      Motors.SetSpeed(arguments[0].toInt());

    if(length == 2)
      Motors.SetSpeed(arguments[0].toInt(), arguments[1].toInt());

    return true;
  }
  */

  return false;
}


int Arguments(String argumentString, String arguments[], int maxLength)
{
  char separator[] = " ";
  int length = 0;

  // First argument
  char *token = strtok(argumentString.c_str(), separator);
  
  while (token != NULL) {

    if(length < maxLength)
      arguments[length] = String(token);

    length++;

    // Next argument
    token = strtok(NULL, separator);
  }

  return length;
}





