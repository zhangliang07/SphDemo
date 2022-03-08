import math
import Partical


_h: float = 5.0
_ad:float = 15.0 / ( 7 * math.pi * _h **2)
#in original method the r = |xi - xj| / h, ∇∇r will become very complacated
#to simplify the computation, how about to assume r = (xi - xj)^2 / h^2 ? 



def kenel(self: 'Partical.Partical', other: 'Partical.Partical') -> float:
  r = math.sqrt((self.posX - other.posX) **2 + (self.posY - other.posY) **2) / _h
  
  w = 0.0;
  if r < 1 :
    w = _ad * (2/3 - r **2 + 0.5 * r**3)
  elif r < 2 :
    w = _ad * 1/6 * (2 - r) **3

  return w



def kenelDerivative(self: 'Partical.Partical', other: 'Partical.Partical'):
  rh = math.sqrt((self.posX - other.posX) **2 + (self.posY - other.posY) **2)
  r = rh / _h

  if(rh == 0.0):
    return 0.0, 0.0

  if r >= 2:
    return 0.0, 0.0

  drdx = 2 * (self.posX - other.posX) / (_h * rh)  #partial derivative of r by x
  drdy = 2 * (self.posY - other.posY) / (_h * rh)  #partial derivative of r by y
  dwdr = 0.0
  if r < 1:
    dwdr = _ad * (3/2 * r **2 - 2 * r)   #partial derivative of w by r
  if r < 2:
    dwdr = - _ad * 1/2 * (2 - r) **2

  dwdx = dwdr * drdx
  dwdy = dwdr * drdy
  return dwdx, dwdy



#deprecated
def kenelDerivative2Order(self: 'Partical.Partical', other: 'Partical.Partical'):
  rh = math.sqrt((self.posX - other.posX) **2 + (self.posY - other.posY) **2)
  r = rh / _h
  dwdr = _ad * (3/2 * r **2 - 2 * r)   #partial derivative of w by r
  d2wdr2 = _ad * (3 * r - 2)

  drdx = 2 * (self.posX - other.posX) / (rh * _h)  #partial derivative of r by x
  drdy = 2 * (self.posY - other.posY) / (rh * _h)  #partial derivative of r by y

  d2rdx2 = (2 / rh - 4 * (self.posX - other.posX) **2 / rh **3) / _h
  d2rdy2 = (2 / rh - 4 * (self.posY - other.posY) **2 / rh **3) / _h
  d2rdxdy = 4 * (self.posX - other.posX) * (self.posY - other.posY) / rh **3 / _h

  #d2wdx2 / dx2 = d2w / dr2 * dr / dx * dr / dx + dw / dx * d2r / dx2
  d2wdx2 = d2wdr2 * drdx * drdx + dwdr * d2rdx2
  d2wdy2 = d2wdr2 * drdy * drdy + dwdr * d2rdy2
  d2wdxdy = d2wdr2 * drdx * drdy + dwdr * d2rdxdy

  return d2wdx2, d2wdy2, d2wdxdy