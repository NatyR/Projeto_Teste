
-- **********************************************************************************
-- *   PORTALRH.T_TIPO_NOTIFICACAO (CREATE) / PORTALRH.T_TIPO_NOTIFICACAO (ALTER)	*
-- *   US 7928 Notificações para RH informar Saldo Bullla na Rescisão - Banner  	*
-- *   Task 8715																	*
-- *   Responsável Renata Felix														*
--***********************************************************************************



--1ª EXECUÇÃO
CREATE TABLE "PORTALRH"."T_TIPO_NOTIFICACAO" 
   (	"ID" NUMBER(38,0) NOT NULL ENABLE, 
	    "NOME" VARCHAR2(100) NOT NULL ENABLE, 
	 CONSTRAINT "T_TIPO_NOTIFICACAO_PK" PRIMARY KEY ("ID")
   );
  
  
--2ª EXECUÇÃO
INSERT ALL
  INTO PORTALRH.T_TIPO_NOTIFICACAO (ID,NOME) VALUES (1,'OUTROS')
  INTO PORTALRH.T_TIPO_NOTIFICACAO (ID,NOME) VALUES (2,'RESCISAO DE COLABORADOR')
  SELECT * FROM dual;



--3ª EXECUÇÃO
ALTER TABLE PORTALRH.T_NOTIFICACAO ADD TIPO_NOTIFICACAO_ID NUMBER(38,0) DEFAULT 1 NOT NULL ENABLE 


