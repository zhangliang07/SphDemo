from turtle import title
import numpy as np  
import matplotlib.pyplot as plt  
  
G = np.array([0, 12000 * -9.8])  
REST_DENS = 1000.  
GAS_CONST = 2000.  
H = 16.  
HSQ = H * H  
MASS = 65.  
VISC = 250.  
DT = 0.0008  
  
M_PI = np.pi  
POLY6 = 315 / (65. * M_PI * pow(H, 9.))  
SPIKY_GRAD = -45 / (M_PI * pow(H, 6.))  
VISC_LAP = 45 / (M_PI * pow(H, 6.))  
  
EPS = H  
BOUND_DAMPING = -0.5  
VIEW_HEIGHT = 800  
VIEW_WIDTH = 1200  
  
  
class Particles:  
    def __init__(self, x: np.ndarray):  
        self.x = x  
        self.v = np.zeros_like(x)  
        self.f = np.zeros_like(x)  
        self.rho = np.zeros(len(x))  
        self.p = np.zeros(len(x))  

  
    @classmethod  
    def initSPH(cls):  
        y = EPS  
        positions = []  
        while y < VIEW_HEIGHT - EPS * 2.:  
            x = EPS  
            while x <= VIEW_WIDTH:  
                if 2 * np.abs(x - 400) ** 2 - 2 * np.abs(x - 400) * (y - 200) + (y - 200) ** 2 <= 30000:  
                    #jitter = np.random.randn()
                    jitter = 0
                    positions.append([x + jitter, y])  
                x += H  
            y += H  
        print(f"Initializing heartbreak with {len(positions)} particles")  
        return cls(x=np.array(positions))  
  
    def computeDensityPressure(self):  
        particles = self  
        for i, particle_i_pos in enumerate(particles.x):  
            particles.rho[i] = 0.  
            for j, particle_j in enumerate(particles.x):  
                rij = particles.x[j, :] - particles.x[i, :]  
                r2 = np.sum(rij * rij)  
                if r2 < HSQ:  
                    particles.rho[i] += MASS * POLY6 * np.power(HSQ - r2, 3.)  
            particles.p[i] = GAS_CONST * (particles.rho[i] - REST_DENS)  
  
    def computeForces(self):  
        particles = self  
        for i, pos_i in enumerate(particles.x):  
            fpress = np.array([0., 0.])  
            fvisc = np.array([0, 0.])  
            for j, pos_j in enumerate(particles.x):  
                if i == j:  
                    continue  
                rij = pos_j - pos_i  
                r = np.linalg.norm(rij)  
                if r < H:  
                    fpress += -rij / r * MASS * (particles.p[i] + particles.p[j]) / (2. * particles.rho[j]) * SPIKY_GRAD * pow(H - r, 2.)  
                    fvisc += VISC * MASS * (particles.v[j, :] - particles.v[i, :]) / particles.rho[j] * VISC_LAP * (H - r)  
            fgrav = G * particles.rho[i]  
            particles.f[i] = fpress + fvisc + fgrav  
  
    def integrate(self):  
        particles = self  
        for i, pos in enumerate(particles.x):  
            particles.v[i, :] += DT * particles.f[i] / particles.rho[i]  
            particles.x[i, :] += DT * particles.v[i, :]  
  
            if pos[0] - EPS < 0.0:  
                particles.v[i, 0] *= BOUND_DAMPING  
                particles.x[i, 0] = EPS  
            if pos[0] + EPS > VIEW_WIDTH:  
                particles.v[i, 0] *= BOUND_DAMPING  
                particles.x[i, 0] = VIEW_WIDTH - EPS  
            if pos[1] - EPS < 0.0:  
                particles.v[i, 1] *= BOUND_DAMPING  
                particles.v[i, 1] += EPS  
                particles.x[i, 1] = EPS  
            if pos[1] + EPS > VIEW_HEIGHT:  
                particles.v[i, 1] *= BOUND_DAMPING  
                particles.v[i, 1] += EPS  
                particles.x[i, 1] = VIEW_HEIGHT - EPS  

 


xx = []
yy = []
particles = Particles.initSPH()
for i, (x, y) in enumerate(particles.x):  
  xx.append(x)
  yy.append(y)

plt.ion()
fig, ax = plt.subplots(1, 1, figsize=(9, 6))  
fig.set_tight_layout(True)

for i in range(230):  
    ax.clear()
    plt.title("step: " + str(i))
    ax.set_aspect("equal")
    ax.set_xlim([0, 1200])
    ax.set_ylim([0, 800])

    particles.computeDensityPressure()
    particles.computeForces()
    particles.integrate() 
    
    for i, (x, y) in enumerate(particles.x):  
      xx[i] = x
      yy[i] = y

    ax.scatter(xx, yy)
    #fig.canvas.draw()
    fig.canvas.flush_events()
    #plt.savefig(f'c_damped_{i}.png')
    #print("saved the picture ", i);

input("Please press the Enter key to proceed")
#plt.ioff()
#plt.show()
