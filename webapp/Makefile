all:
	[ -d node_modules ] || npm install
	npm run build

DST = ../apps/ScadView/app

install: all
	@echo "Cleaning webapp directory..."
	rm -rf $(DST)/js
	mkdir -p $(DST)/js
	cp index.html $(DST)
	cp js/* $(DST)/js/

