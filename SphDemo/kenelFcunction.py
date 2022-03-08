import math
import Partical


__h: float = 10.0
__ad:float = 15 / ( 7 * math.pi * __h **2)
#in original method the r = |xi - xj| / h, ∇∇r will become very complacated
#to simplify the computation, how about to assume r = (xi - xj)^2 / h^2 ? 



def kenel(self: Partical.Partical, other: Partical.Partical) -> float:
  r = math.sqrt((self.posX - other.posX) **2 + (self.posY - other.posY) **2) / __h
  
  w = 0;
  if r < 1 :
    w = __ad * (2/3 - r **2 + 0.5 * r**3)
  elif r < 2 :
    w = __ad * 1/6 * (2 - r) **3

  return w



def kenelDerivative(self: Partical.Partical, other: Partical.Partical):
  rh = math.sqrt((self.posX - other.posX) **2 + (self.posY - other.posY) **2)
  r = rh / __h

  dwdx, dwdy = 0
  if r >= 2:
    pass

  drdx = 2 * (self.posX - other.posX) / (__h * rh)  #partial derivative of r by x
  drdy = 2 * (self.posY - other.posY) / (__h * rh)  #partial derivative of r by y
  if r < 1:
    dwdr = __ad * (3/2 * r **2 - 2 * r)   #partial derivative of w by r
  if r < 2:
    dwdr = - __ad * 1/2 * (2 - r) **2

  dwdx = dwdr * drdx
  dwdy = dwdr * drdy
  return dwdx, dwdy



#deprecated
def kenelDerivative2Order(self: Partical.Partical, other: Partical.Partical):
  rh = math.sqrt((self.posX - other.posX) **2 + (self.posY - other.posY) **2)
  r = rh / __h
  dwdr = __ad * (3/2 * r **2 - 2 * r)   #partial derivative of w by r
  d2wdr2 = __ad * (3 * r - 2)

  drdx = 2 * (self.posX - other.posX) / (rh * __h)  #partial derivative of r by x
  drdy = 2 * (self.posY - other.posY) / (rh * __h)  #partial derivative of r by y

  d2rdx2 = (2 / rh - 4 * (self.posX - other.posX) **2 / rh **3) / __h
  d2rdy2 = (2 / rh - 4 * (self.posY - other.posY) **2 / rh **3) / __h
  d2rdxdy = 4 * (self.posX - other.posX) * (self.posY - other.posY) / rh **3 / __h

  #d2wdx2 / dx2 = d2w / dr2 * dr / dx * dr / dx + dw / dx * d2r / dx2
  d2wdx2 = d2wdr2 * drdx * drdx + dwdr * d2rdx2
  d2wdy2 = d2wdr2 * drdy * drdy + dwdr * d2rdy2
  d2wdxdy = d2wdr2 * drdx * drdy + dwdr * d2rdxdy

  return d2wdx2, d2wdy2, d2wdxdy