clear all
close all
clc

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
% Symulator Układu Chłodzącego
% (c) Maciej Paszek 2025
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
% Parametry symulatora
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

% Temperatura maksymalna (otoczenia)
TempMax = 25;

% Temperatura minimalna 
TempMin = 8;

% Temperatura początkowa
Temp0   = 20;

% Stała czasowa układu
T      = 100;

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
% Parametry symulacji
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

% Liczba iteracji
N = 8000;

% Krok całkowania
h = 0.1;

% Wymuszenie
u = [zeros(N/4, 1); ones(N/4, 1); zeros(N/4, 1); zeros(N/4, 1)];

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
% Zmienne symulacji
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

% Wzmocnienie pierwszej inercji
k = TempMax - TempMin;

% Warunki początkowe
xPrev = Temp0 - TempMin;
yPrev = Temp0 - TempMin;

% Wartości wykresu
y = zeros(N, 1);

for t = 1:N
    
    % Sterowanie
    uNew = u(t);

    % Inercja I - na wejście wchodzi sterowanie
    xNew = Inercja(xPrev, uNew, k, T, h);
    
    % Inercja II - na wejście wchodzi wyjście poprzedniej inercji
    yNew = Inercja(yPrev, xNew, 1, T, h);

    % Wyjście przesunięte o wartość podporową
    y(t) = yNew + TempMin;

    % Przepisanie wartości
    yPrev = yNew;
    xPrev = xNew;
end

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
% Wykres
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

% Argumenty wykresu
t = 1:N;

figure
plot(t, u, t, y);
xlabel("t, s");
ylabel("T, C");
legend(["u(t)", "y(t)"]);
grid on;

function [yNew] = Inercja(yPrev, u, k, T, h)

    RHS = (k * u - yPrev) / T;
    yNew = yPrev + RHS * h;

end