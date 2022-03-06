import math
import Partical



__h = Partical.__h



def kenelFunction1(self, other: Partical.Partical):
  ad = 5 / ( math.pi * __h **2)

  r = math.sqrt((self.posX - other.posX) **2 + (self.posY - other.posY) **2) / __h
  
  w = 0;
  if r <= 1 :
    w = ad * (1 + 3 * r) * (1 - r) **3

  return w



def kenelFunction1derivative(self, other: Partical.Partical):
  ad = 5 /( math.pi * __h **2)

  r = math.sqrt((self.posX - other.posX) **2 + (self.posY - other.posY) **2) / __h
  dwdr = ad * (-12 * r **3 + 24 * r **2 - 12 * r)  #partial derivative of w by r

  divide = math.sqrt(other.posX **2 - 2 * self.posX * other.posX + self.posX **2 \
    + other.posY **2 - 2 * self.posY * other.posY + self.posY **2) * __h
  drdx = - (self.posX - other.posX) / divide  #partial derivative of r by x
  drdy = - (self.posY - other.posY) / divide  #partial derivative of r by y

  dwdx = dwdr * drdx
  dwdy = dwdr * drdy
  return dwdx, dwdy
