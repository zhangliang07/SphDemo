import math
import Partical


__h: float = 10
__ad = 15 / ( 7 * math.pi * __h **2)
#in original method the r = |xi - xj| / h, ∇∇r will become very complacated
#here I assume r = (xi - xj)^2 / h2, to simplify the computation



def kenel(self: Partical.Partical, other: Partical.Partical) -> float:
  r = ((self.posX - other.posX) **2 + (self.posY - other.posY) **2) / __h **2
  
  w = 0;
  if r < 1 :
    w = __ad * (2/3 - r **2 + 0.5 * r**3)
  elif r < 2 :
    w = __ad * 1/6 * (2 - r) **3

  return w



def kenelDerivative(self: Partical.Partical, other: Partical.Partical):
  r = ((self.posX - other.posX) **2 + (self.posY - other.posY) **2) / __h **2
  dwdr = __ad * (3/2 * r **2 - 2 * r)   #partial derivative of w by r

  drdx = -2 * (self.posX - other.posX) / __h **2  #partial derivative of r by x
  drdy = -2 * (self.posY - other.posY) / __h **2  #partial derivative of r by y

  dwdx = dwdr * drdx
  dwdy = dwdr * drdy
  return dwdx, dwdy



def kenelDerivative2order(self: Partical.Partical, other: Partical.Partical):
  r = ((self.posX - other.posX) **2 + (self.posY - other.posY) **2) / __h **2
  dwdr = __ad * (3/2 * r **2 - 2 * r)   #partial derivative of w by r
  d2wdr2 = __ad * (3 * r - 2)   #partial derivative of w by r

  divide = math.sqrt(other.posX **2 - 2 * self.posX * other.posX + self.posX **2 \
    + other.posY **2 - 2 * self.posY * other.posY + self.posY **2) * __h
  drdx = - (self.posX - other.posX) / divide  #partial derivative of r by x
  drdy = - (self.posY - other.posY) / divide  #partial derivative of r by y

  dwdx = dwdr * drdx
  dwdy = dwdr * drdy
  return dwdx, dwdy