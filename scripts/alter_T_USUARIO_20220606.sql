ALTER TABLE PORTALRH.T_USUARIO ADD ID_TIPO NUMBER DEFAULT 1;

UPDATE PORTALRH.T_USUARIO SET ID_TIPO = 2 WHERE PERFIL_ID = 2;

INSERT INTO PORTALRH.T_PERFIL (ID,DESCRICAO,SISTEMA_ID) VALUES (S_PERFIL.nextval,'OPERACIONAL',2);

INSERT INTO PORTALRH.T_PERFIL (ID,DESCRICAO,SISTEMA_ID) VALUES (S_PERFIL.nextval,'FINANCEIRO',2);

INSERT INTO PORTALRH.T_PERFIL (ID,DESCRICAO,SISTEMA_ID) VALUES (S_PERFIL.nextval,'CONSULTA',2);

ALTER TABLE PORTALRH.T_MENU ADD SISTEMA_ID NUMBER NOT NULL;

ALTER TABLE PORTALRH.T_MENU ADD URL VARCHAR2(100) DEFAULT '';

INSERT INTO PORTALRH.T_MENU (ID,NOME,TIPO,PARENT,ICONE,ORDEM,SISTEMA_ID,URL) VALUES
	 (3,'Emissão cartão em lote','MENU',1,NULL,3,2,'/cards/import'),
	 (1,'Colaboradores','GRUPO',NULL,'Users',1,2,NULL),
	 (2,'Emissão cartão individual','MENU',1,NULL,2,2,'/cards/new'),
	 (4,'Status do pedido','MENU',1,NULL,4,2,'/cards/status'),
	 (5,'Cartões','MENU',1,NULL,5,2,'/cards/list'),
	 (6,'Empresa','GRUPO',NULL,'Company',6,2,NULL),
	 (7,'Dados do convênio','MENU',6,NULL,7,2,'/'),
	 (8,'Financeiro','MENU',6,NULL,8,2,'/'),
	 (9,'Nota fiscal','MENU',6,NULL,9,2,'/'),
	 (10,'Limite global','MENU',6,NULL,10,2,'/');
INSERT INTO PORTALRH.T_MENU (ID,NOME,TIPO,PARENT,ICONE,ORDEM,SISTEMA_ID,URL) VALUES
	 (11,'Relatórios','GRUPO',NULL,'Report',11,2,NULL),
	 (12,'Limite','MENU',11,NULL,12,2,'/reports/limit'),
	 (13,'Acessos Portal','GRUPO',NULL,'Users',13,2,NULL),
	 (14,'Criar Acesso Portal','MENU',13,NULL,14,2,'/users/new/2'),
	 (15,'Gerenciar Acessos Portal','MENU',13,NULL,15,2,'/users/list/2'),
	 (16,'Acessos Portal','GRUPO',NULL,'Users',16,1,NULL),
	 (17,'Criar Acesso Portal','MENU',16,NULL,17,1,'/users/new/2'),
	 (18,'Gerenciar Acessos Portal','MENU',16,NULL,18,1,'/users/list/2'),
	 (19,'Acessos Gerenciador','GRUPO',NULL,'Users',19,1,NULL),
	 (20,'Criar Acesso Gerenciador','MENU',19,NULL,20,1,'/users/new/1');
INSERT INTO PORTALRH.T_MENU (ID,NOME,TIPO,PARENT,ICONE,ORDEM,SISTEMA_ID,URL) VALUES
	 (21,'Gerenciar Acessos Gerenciador','MENU',19,NULL,21,1,'/users/list/1'),
	 (22,'Perfil de Acesso','GRUPO',NULL,'Users',22,1,NULL),
	 (23,'Criar Perfil de Acesso','MENU',22,NULL,23,1,'/profiles/new'),
	 (24,'Perfis de Acesso','MENU',22,NULL,24,1,'/profiles/list');


CREATE TABLE "PORTALRH"."T_USUARIO_CONVENIO" 
   (	"USUARIO_ID" NUMBER, 
	"CONVENIO_ID" NUMBER, 
	"PERFIL_ID" NUMBER
   )