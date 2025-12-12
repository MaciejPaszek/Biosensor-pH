#ifndef INERCJA_H
#define INERCJA_H

#include <Arduino.h>

class INERCJA {

  public:
    // Konstruktor bezparametrowy
    INERCJA();

    // Konstruktor z zerowymi warunkami początkowymi
    INERCJA(double k, double T);

    // Konstruktor z niezerowymi warunkami początkowymi
    INERCJA(double k, double T, double yPrev);

    // Krok metody Eulera
    double Euler(double u, double h);

  private:

    // Parametry Inercji
    double k;
    double T;
    double h;

    // Stan wewnętrzny
    double yPrev;
};

#endif