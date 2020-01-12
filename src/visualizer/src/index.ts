enum MoveType {
    TableauToFoundation = 0,
    TableauToReserve = 1,
    TableauToTableau = 2,
    ReserveToFoundation = 3,
    ReserveToTableau = 4,
};

type Move = { type: MoveType, from: number, to: number, size: number };

class ReplayBoard {
    cardWidthWithSpacing = 175;
    stackOffset = 60;

    zIndex = 0;

    slowDown: HTMLButtonElement;
    speedUp: HTMLButtonElement;
    prev: HTMLButtonElement;
    playPause: HTMLButtonElement;
    next: HTMLButtonElement;

    r: Element;
    f: Element;
    t: Element;

    movesAnimArray: { translateX: number, translateY: number }[] = [];

    currentPos = 0;
    isPlaying = false;

    rLeft = 0;
    fLeft = 0;
    tLeft = 0;

    duration = 850;

    constructor(public board: number[][], public moves: Move[]) {
        this.slowDown = document.getElementById('slow-down')! as HTMLButtonElement;
        this.speedUp = document.getElementById('speed-up')! as HTMLButtonElement;
        this.prev = document.getElementById('prev')! as HTMLButtonElement;
        this.playPause = document.getElementById('play-pause')! as HTMLButtonElement;
        this.next = document.getElementById('next')! as HTMLButtonElement;
        this.slowDown.onclick = () => this.duration < 850 ? this.duration += 250 : null;
        this.speedUp.onclick = () => this.duration > 100 ? this.duration -= 250 : null;
        this.prev.onclick = this.stepBackward;
        this.playPause.onclick = this.playOrPause;
        this.next.onclick = this.stepForward;

        this.r = document.getElementsByClassName('reserve')[0];
        this.f = document.getElementsByClassName('foundation')[0];
        this.t = document.getElementsByClassName('tableaus')[0];
        window.onresize = this.onResize;
        this.onResize();
        this.initializeBoard();
    }

    private playOrPause = () => {
        this.isPlaying = !this.isPlaying;

        if (!this.isPlaying) {
            this.playPause.children[0].setAttribute('style', '');
            this.playPause.children[1].setAttribute('style', 'display:none;');
        } else {
            this.playPause.children[0].setAttribute('style', 'display:none;');
            this.playPause.children[1].setAttribute('style', '');
            this.play();
        }
    }

    private stepBackward = async () => {
        if (this.currentPos > 0) {
            if (this.isPlaying) {
                // Pause first
                this.playOrPause();
                // Wait for any pending animation to complete
                await this.delay(this.duration + 50);
            }
            await this.undo(this.moves[--this.currentPos], this.movesAnimArray[this.currentPos]);
        }
    }

    private play = async () => {
        for (let i = this.currentPos; i < this.moves.length && this.isPlaying; i++) {
            await this.move(this.moves[this.currentPos++]);
        }
        if (this.currentPos == this.moves.length) {
            this.playOrPause(); // Pause when done
        }
    }

    private stepForward = async () => {
        if (this.currentPos < this.moves.length) {
            if (this.isPlaying) {
                // Pause first
                this.playOrPause();
                // Wait for any pending animation to complete
                await this.delay(this.duration + 50);
            }
            await this.move(this.moves[this.currentPos++]);
        }
    }

    private initializeBoard() {
        for (let i = 0; i < 8; i++) {
            const t = this.t.children[i];
            for (let j = 0; j < this.board[i].length; j++) {
                const val = this.board[i][j];

                const img = document.createElement('img');
                img.src = `assets/${val}.png`;
                img.className = `rv${val}`;
                img.dataset.value = val.toString();
                img.setAttribute('style', `top:${(j * this.stackOffset)}px;`);

                t.appendChild(img);
            }
        }
    }

    private getTableauSize(index: number) {
        return this.t.children[index].children.length - 2;
    }

    private getTableauCards(index: number, size = 1) {
        const tableauCards = Array
            .from(this.t.children[index].children)
            .slice(-size)
            .map(el => parseInt((el as HTMLElement).dataset.value!));

        return tableauCards;
    }

    private getReserveCard(index: number) {
        return parseInt((this.r.children[index].children[1] as HTMLElement).dataset.value!);
    }

    private getFoundationCard(index: number) {
        const cards = this.f.children[index].children;
        return parseInt((cards[cards.length - 1] as HTMLElement).dataset.value!);
    }

    private async move(m: Move) {
        let sourceX: number, sourceY: number, targetX: number, targetY: number;
        let translateX: number, translateY: number;
        let cards: number[];
        let target: Element;

        switch (m.type) {
            case MoveType.TableauToFoundation:
                target = this.f.children[m.to];

                sourceX = this.tLeft + (this.cardWidthWithSpacing * m.from);
                sourceY = 350 + this.getTableauSize(m.from) * this.stackOffset;
                targetX = this.fLeft + (this.cardWidthWithSpacing * m.to);
                targetY = 50;

                cards = [this.getTableauCards(m.from)[0]];
                break;
            case MoveType.TableauToReserve:
                target = this.r.children[m.to];

                sourceX = this.tLeft + (this.cardWidthWithSpacing * m.from);
                sourceY = 350 + this.getTableauSize(m.from) * this.stackOffset;
                targetX = this.rLeft + (this.cardWidthWithSpacing * m.to);
                targetY = 50;

                cards = [this.getTableauCards(m.from)[0]];
                break;
            case MoveType.TableauToTableau:
                target = this.t.children[m.to];

                sourceX = this.tLeft + (this.cardWidthWithSpacing * m.from);
                sourceY = 350 + (this.getTableauSize(m.from) - m.size) * this.stackOffset;
                targetX = this.tLeft + (this.cardWidthWithSpacing * m.to);
                targetY = 350 + this.getTableauSize(m.to) * this.stackOffset;

                cards = this.getTableauCards(m.from, m.size);
                break;
            case MoveType.ReserveToFoundation:
                target = this.f.children[m.to];

                sourceX = this.rLeft + (this.cardWidthWithSpacing * m.from);
                sourceY = 50;
                targetX = this.fLeft + (this.cardWidthWithSpacing * m.to);
                targetY = 50;

                cards = [this.getReserveCard(m.from)];
                break;
            case MoveType.ReserveToTableau:
                target = this.t.children[m.to];

                sourceX = this.rLeft + (this.cardWidthWithSpacing * m.from);
                sourceY = 50;
                targetX = this.tLeft + (this.cardWidthWithSpacing * m.to);
                targetY = 350 + (this.getTableauSize(m.to) + 1) * this.stackOffset;

                cards = [this.getReserveCard(m.from)];
                break;
        }

        translateX = targetX - sourceX;
        translateY = targetY - sourceY;
        this.movesAnimArray.push({ translateX, translateY });

        const cardEls = cards.map(v => document.getElementsByClassName(`rv${v}`)[0]);
        cardEls.forEach(el => el.setAttribute('style', el.getAttribute('style') + `z-index:${++this.zIndex}`));

        const duration = this.duration;
        await anime({
            targets: cards.map(rv => `.rv${rv}`),
            translateX,
            translateY,
            easing: 'easeInOutSine',
            duration,
        }).finished;

        if (m.type == MoveType.ReserveToTableau || m.type == MoveType.TableauToTableau) {
            for (let i = 0; i < cardEls.length; i++) {
                cardEls[i].setAttribute('style', `top:${(target.children.length - 1) * this.stackOffset}px;`);
                target.appendChild(cardEls[i]);
            }
        } else {
            cardEls[0].setAttribute('style', '');
            target.appendChild(cardEls[0]);
        }
    }

    private async undo(m: Move, anim: { translateX: number, translateY: number }) {
        let cards: number[];
        let target: Element;

        switch (m.type) {
            case MoveType.TableauToFoundation:
                target = this.t.children[m.from];
                cards = [this.getFoundationCard(m.to)];
                break;
            case MoveType.TableauToReserve:
                target = this.t.children[m.from];
                cards = [this.getReserveCard(m.to)];
                break;
            case MoveType.TableauToTableau:
                target = this.t.children[m.from];
                cards = this.getTableauCards(m.to, m.size);
                break;
            case MoveType.ReserveToFoundation:
                target = this.r.children[m.from];
                cards = [this.getFoundationCard(m.to)];
                break;
            case MoveType.ReserveToTableau:
                target = this.r.children[m.from];
                cards = this.getTableauCards(m.to);
                break;
        }

        const cardEls = cards.map(v => document.getElementsByClassName(`rv${v}`)[0]);
        cardEls.forEach(el => el.setAttribute('style', el.getAttribute('style') + `z-index:${++this.zIndex}`));

        const duration = this.duration;
        await anime({
            targets: cards.map(rv => `.rv${rv}`),
            translateX: -anim.translateX,
            translateY: -anim.translateY,
            easing: 'easeInOutSine',
            duration,
        }).finished;

        this.movesAnimArray.pop();

        if (m.type == MoveType.TableauToFoundation || m.type == MoveType.TableauToReserve || m.type == MoveType.TableauToTableau) {
            for (let i = 0; i < cardEls.length; i++) {
                cardEls[i].setAttribute('style', `top:${(target.children.length - 1) * this.stackOffset}px;`);
                target.appendChild(cardEls[i]);
            }
        } else {
            cardEls[0].setAttribute('style', '');
            target.appendChild(cardEls[0]);
        }
    }

    private onResize = () => {
        const minWidth = 1540;
        if (innerWidth > minWidth) {
            const hDiff = (innerWidth - minWidth) / 2;
            this.rLeft = 20 + hDiff;
            this.fLeft = 845 + hDiff;
            this.tLeft = 82 + hDiff;

            this.r.setAttribute('style', `left:${this.rLeft}px;top:50px;`);
            this.f.setAttribute('style', `left:${this.fLeft}px;top:50px;`);
            this.t.setAttribute('style', `left:${this.tLeft}px;top:350px;`);
        }
    }

    private async delay(ms: number) {
        return new Promise(resolve => setTimeout(() => resolve(), ms));
    }
}