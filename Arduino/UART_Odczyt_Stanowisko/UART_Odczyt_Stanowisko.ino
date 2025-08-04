#include <DHT.h>
#include <DHT_U.h>
#include <OneWire.h>
#include <DallasTemperature.h>

// DHT11 ustawienia
#define DHTPIN 2
#define DHTTYPE DHT11
DHT dht(DHTPIN, DHTTYPE);

// DS18B20 ustawienia
#define ONE_WIRE_BUS 3  // Pin cyfrowy dla DS18B20
OneWire oneWire(ONE_WIRE_BUS);
DallasTemperature sensors(&oneWire);

void setup() {
  Serial.begin(9600);
  dht.begin();
  sensors.begin();
}

void loop() {
  // Odczyt DHT11
  float AmbientTemperature = dht.readTemperature();
  float AmbientHumidity = dht.readHumidity();

  if (isnan(AmbientTemperature) || isnan(AmbientHumidity)) {
    AmbientTemperature = -1;
    AmbientHumidity = -1;
  }

  // Odczyt DS18B20
  sensors.requestTemperatures();
  float SampleTemperature = sensors.getTempCByIndex(0);

  if (SampleTemperature == DEVICE_DISCONNECTED_C) {
    SampleTemperature = -1;
  }

  Serial.print(SampleTemperature);
  Serial.print(" ");
  Serial.print(AmbientTemperature);
  Serial.print(" ");
  Serial.println(AmbientHumidity);
 
  delay(1);
}
