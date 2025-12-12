#include "Inercja.h"

// Konstruktor bezparametrowy
INERCJA::INERCJA() {}

// Konstruktor z zerowymi warunkami początkowymi
INERCJA::INERCJA(double k, double T) {

  this->k     = k;
  this->T     = T;
  this->yPrev = 0;

}

// Konstruktor z niezerowymi warunkami początkowymi
INERCJA::INERCJA(double k, double T, double yPrev) {

  this->k     = k;
  this->T     = T;
  this->yPrev = yPrev;
  
}

// Krok metody Eulera
double INERCJA::Euler(double u, double h)
{
  double RHS = (k * u - yPrev) / T;
  double yNew = yPrev + RHS * h;
  
  yPrev = yNew;

  return yNew;
}



