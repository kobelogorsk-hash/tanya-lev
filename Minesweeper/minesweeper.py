import pygame
import random
import sys

# Инициализация Pygame
pygame.init()

# Константы
CELL_SIZE = 30
GRID_WIDTH = 10
GRID_HEIGHT = 10
NUM_MINES = 15

# Цвета
WHITE = (255, 255, 255)
BLACK = (0, 0, 0)
GRAY = (200, 200, 200)
DARK_GRAY = (150, 150, 150)
RED = (255, 0, 0)
GREEN = (0, 255, 0)
BLUE = (0, 0, 255)
YELLOW = (255, 255, 0)
CYAN = (0, 255, 255)
MAGENTA = (255, 0, 255)
ORANGE = (255, 165, 0)

# Числа цветов
NUMBER_COLORS = {
    1: BLUE,
    2: GREEN,
    3: RED,
    4: DARK_GRAY,
    5: MAGENTA,
    6: CYAN,
    7: BLACK,
    8: ORANGE
}

# Настройка экрана
WIDTH = GRID_WIDTH * CELL_SIZE
HEIGHT = GRID_HEIGHT * CELL_SIZE
screen = pygame.display.set_mode((WIDTH, HEIGHT))
pygame.display.set_caption("Сапер")

clock = pygame.time.Clock()
font = pygame.font.SysFont('Arial', 20, bold=True)

class Cell:
    def __init__(self, x, y):
        self.x = x
        self.y = y
        self.is_mine = False
        self.is_revealed = False
        self.is_flagged = False
        self.neighbor_mines = 0

    def reveal(self):
        self.is_revealed = True

    def toggle_flag(self):
        self.is_flagged = not self.is_flagged

class Minesweeper:
    def __init__(self):
        self.grid = [[Cell(x, y) for y in range(GRID_HEIGHT)] for x in range(GRID_WIDTH)]
        self.game_over = False
        self.won = False
        self.place_mines()
        self.calculate_neighbors()

    def place_mines(self):
        mines_placed = 0
        while mines_placed < NUM_MINES:
            x = random.randint(0, GRID_WIDTH - 1)
            y = random.randint(0, GRID_HEIGHT - 1)
            if not self.grid[x][y].is_mine:
                self.grid[x][y].is_mine = True
                mines_placed += 1

    def calculate_neighbors(self):
        for x in range(GRID_WIDTH):
            for y in range(GRID_HEIGHT):
                if not self.grid[x][y].is_mine:
                    count = 0
                    for dx in [-1, 0, 1]:
                        for dy in [-1, 0, 1]:
                            nx, ny = x + dx, y + dy
                            if 0 <= nx < GRID_WIDTH and 0 <= ny < GRID_HEIGHT:
                                if self.grid[nx][ny].is_mine:
                                    count += 1
                    self.grid[x][y].neighbor_mines = count

    def reveal_cell(self, x, y):
        if not (0 <= x < GRID_WIDTH and 0 <= y < GRID_HEIGHT):
            return
        cell = self.grid[x][y]
        if cell.is_revealed or cell.is_flagged:
            return
        
        cell.reveal()
        
        if cell.is_mine:
            self.game_over = True
            self.reveal_all_mines()
        elif cell.neighbor_mines == 0:
            for dx in [-1, 0, 1]:
                for dy in [-1, 0, 1]:
                    nx, ny = x + dx, y + dy
                    self.reveal_cell(nx, ny)

    def reveal_all_mines(self):
        for x in range(GRID_WIDTH):
            for y in range(GRID_HEIGHT):
                if self.grid[x][y].is_mine:
                    self.grid[x][y].reveal()

    def check_win(self):
        for x in range(GRID_WIDTH):
            for y in range(GRID_HEIGHT):
                cell = self.grid[x][y]
                if not cell.is_mine and not cell.is_revealed:
                    return False
        self.won = True
        self.game_over = True
        return True

    def draw(self):
        screen.fill(GRAY)
        for x in range(GRID_WIDTH):
            for y in range(GRID_HEIGHT):
                cell = self.grid[x][y]
                rect = pygame.Rect(x * CELL_SIZE, y * CELL_SIZE, CELL_SIZE, CELL_SIZE)
                
                if cell.is_revealed:
                    if cell.is_mine:
                        pygame.draw.rect(screen, RED, rect)
                        # Рисуем мину
                        center = (x * CELL_SIZE + CELL_SIZE // 2, y * CELL_SIZE + CELL_SIZE // 2)
                        pygame.draw.circle(screen, BLACK, center, CELL_SIZE // 3)
                    else:
                        pygame.draw.rect(screen, WHITE, rect)
                        if cell.neighbor_mines > 0:
                            color = NUMBER_COLORS.get(cell.neighbor_mines, BLACK)
                            text = font.render(str(cell.neighbor_mines), True, color)
                            text_rect = text.get_rect(center=(x * CELL_SIZE + CELL_SIZE // 2, 
                                                              y * CELL_SIZE + CELL_SIZE // 2))
                            screen.blit(text, text_rect)
                else:
                    pygame.draw.rect(screen, GRAY, rect)
                    pygame.draw.rect(screen, DARK_GRAY, rect, 2)
                    
                    if cell.is_flagged:
                        # Рисуем флаг
                        center = (x * CELL_SIZE + CELL_SIZE // 2, y * CELL_SIZE + CELL_SIZE // 2)
                        pygame.draw.polygon(screen, RED, [
                            (x * CELL_SIZE + CELL_SIZE // 2, y * CELL_SIZE + 5),
                            (x * CELL_SIZE + CELL_SIZE // 2 + 10, y * CELL_SIZE + 12),
                            (x * CELL_SIZE + CELL_SIZE // 2, y * CELL_SIZE + 20)
                        ])
                        pygame.draw.line(screen, BLACK, 
                                        (x * CELL_SIZE + CELL_SIZE // 2, y * CELL_SIZE + 5),
                                        (x * CELL_SIZE + CELL_SIZE // 2, y * CELL_SIZE + 25), 2)
                
                pygame.draw.rect(screen, BLACK, rect, 1)
        
        if self.game_over:
            self.show_game_over()

    def show_game_over(self):
        overlay = pygame.Surface((WIDTH, HEIGHT))
        overlay.set_alpha(200)
        overlay.fill(BLACK)
        screen.blit(overlay, (0, 0))
        
        if self.won:
            text = font.render("ПОБЕДА!", True, GREEN)
        else:
            text = font.render("ИГРА ОКОНЧЕНА", True, RED)
        
        text_rect = text.get_rect(center=(WIDTH // 2, HEIGHT // 2))
        screen.blit(text, text_rect)
        
        restart_text = font.render("Нажмите R для рестарта", True, WHITE)
        restart_rect = restart_text.get_rect(center=(WIDTH // 2, HEIGHT // 2 + 40))
        screen.blit(restart_text, restart_rect)

def main():
    game = Minesweeper()
    running = True
    
    while running:
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                running = False
            
            if event.type == pygame.MOUSEBUTTONDOWN and not game.game_over:
                mx, my = pygame.mouse.get_pos()
                x = mx // CELL_SIZE
                y = my // CELL_SIZE
                
                if event.button == 1:  # Левая кнопка - открыть
                    game.reveal_cell(x, y)
                    game.check_win()
                elif event.button == 3:  # Правая кнопка - флаг
                    game.grid[x][y].toggle_flag()
            
            if event.type == pygame.KEYDOWN:
                if event.key == pygame.K_r and game.game_over:
                    game = Minesweeper()
        
        game.draw()
        pygame.display.flip()
        clock.tick(30)
    
    pygame.quit()
    sys.exit()

if __name__ == "__main__":
    main()
